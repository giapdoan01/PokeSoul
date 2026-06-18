using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class LunelleSkill : MonoBehaviour, IPokemonSkillLaunch
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog;

    [Header("Projectile")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("VFX")]
    [SerializeField] private GameObject skillImpactVFX;
    [SerializeField] private GameObject enchantedImpactVFX;
    [SerializeField] private float impactLifetime = 1.5f;
    [SerializeField] private int impactPreloadAmount = 3;

    private const string SkillImpactPoolKey = "LunelleImpact";
    private const string EnchantedImpactPoolKey = "LunelleEnchantedImpact";

    [Header("SFX")]
    public AudioClip skillSFX;
    public AudioClip impactSFX;
    public AudioSource audioSource;

    private PokemonSkill _ownerSkill;
    private Transform _target;
    private EnemyMoveController _targetMove;
    private double _runtimeDamage;
    private float _enchantTime;
    private bool _didHit;

    private Collider[] _cachedColliders;
    private Rigidbody _cachedRb;
    private CancellationTokenSource _cts;

    // ── Lifecycle ──

    private void Awake()
    {
        _cachedColliders = GetComponentsInChildren<Collider>(true);
        _cachedRb = GetComponent<Rigidbody>();
        ConfigurePhysics();
        RegisterPools();
    }

    private void RegisterPools()
    {
        if (SkillObjectPolling.Instance == null) return;
        if (skillImpactVFX != null)
            SkillObjectPolling.Instance.RegisterPrefab(SkillImpactPoolKey, skillImpactVFX, impactPreloadAmount);
        if (enchantedImpactVFX != null)
            SkillObjectPolling.Instance.RegisterPrefab(EnchantedImpactPoolKey, enchantedImpactVFX, impactPreloadAmount);
    }

    // ── IPokemonSkillLaunch ──

    public void Launch(PokemonSkill owner, Transform targetEnemy, PokemonData pokemonData, int level, string attack)
    {
        _ownerSkill = owner;
        _target = targetEnemy;
        _targetMove = targetEnemy?.GetComponent<EnemyMoveController>();
        _runtimeDamage = ResolveStat(pokemonData, level, "damage");
        _enchantTime = (float)ResolveStat(pokemonData, level, "enchanttime");
        if (_enchantTime <= 0f) _enchantTime = 2f;
        _didHit = false;

        ConfigurePhysics();
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        audioSource?.PlayOneShot(skillSFX);
        gameObject.SetActive(true);
        LogDebug($"Launch. target={_target?.name}, dmg={_runtimeDamage}, enchant={_enchantTime}s");
    }

    // ── Movement ──

    private void Update()
    {
        if (_didHit) return;
        if (_target == null || !_target.gameObject.activeInHierarchy) { Release(); return; }

        Vector3 direction = _target.position - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDir = direction.normalized;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
            transform.forward = moveDir;
        }
    }

    // ── Hit ──

    private void OnTriggerEnter(Collider other) => HandleHit(other);

    private void OnCollisionEnter(Collision collision) => HandleHit(collision?.collider);

    private void HandleHit(Collider other)
    {
        if (_didHit || other == null) return;
        if (!IsTarget(other)) return;
        _didHit = true;

        // Damage
        var hp = other.GetComponentInParent<EnemyHPController>() ?? other.GetComponent<EnemyHPController>();
        hp?.TakeDamage(_runtimeDamage);

        // Enchant
        var status = other.GetComponentInParent<EnemyStatusEffects>() ?? other.GetComponent<EnemyStatusEffects>();
        status?.Enchant(_enchantTime);

        LogDebug($"Hit. target={_target?.name}, dmg={_runtimeDamage}, enchant={_enchantTime}s");

        // Hit impact
        SpawnVFX(SkillImpactPoolKey, skillImpactVFX, _target != null ? _target.position : transform.position, true);

        // Enchanted impact bám theo enemy
        EnchantedImpactFollowAsync(_target, _enchantTime, _cts.Token).Forget();

        MonImpactSoundManager.Instance?.PlaySound(impactSFX);
        Release();
    }

    private bool IsTarget(Collider other)
    {
        if (other == null) return false;
        Transform t = other.transform;
        if (_target != null && (t == _target || t.IsChildOf(_target) || _target.IsChildOf(t))) return true;
        return _ownerSkill != null && other.CompareTag(_ownerSkill.EnemyTag);
    }

    // ── Enchanted Impact tracking ──

    private async UniTaskVoid EnchantedImpactFollowAsync(Transform enemy, float duration, CancellationToken ct)
    {
        if (enemy == null || enchantedImpactVFX == null) return;

        var impactGo = SpawnVFX(EnchantedImpactPoolKey, enchantedImpactVFX, enemy.position, false);
        if (impactGo == null) return;

        // Safety: tự return sau duration + 1s nếu async tracking thất bại
        var safetyToken = impactGo.GetComponent<SkillPoolToken>();
        if (safetyToken != null) safetyToken.ReturnToPoolAfterDelay(duration + 1f);

        float elapsed = 0f;
        var targetMove = enemy.GetComponent<EnemyMoveController>();

        while (elapsed < duration && enemy != null && enemy.gameObject.activeInHierarchy)
        {
            if (ct.IsCancellationRequested) break;

            Vector3 pos = targetMove != null ? targetMove.FootPosition : enemy.position;
            impactGo.transform.position = pos + Vector3.up * 0.5f;

            await UniTask.Delay(50, cancellationToken: ct);
            elapsed += 0.05f;
        }

        // Cleanup — hủy safety token và return ngay
        if (impactGo != null && impactGo.activeInHierarchy)
        {
            safetyToken?.CancelAutoReturn();

            var ps = impactGo.GetComponentInChildren<ParticleSystem>();
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (SkillObjectPolling.Instance != null)
                SkillObjectPolling.Instance.ReturnByInstance(impactGo);
            else
                Destroy(impactGo);
        }
    }

    // ── Helpers ──

    private GameObject SpawnVFX(string poolKey, GameObject fallbackPrefab, Vector3 position, bool autoReturn)
    {
        if (fallbackPrefab == null) return null;

        RegisterPools();
        GameObject go = SkillObjectPolling.Instance != null
            ? SkillObjectPolling.Instance.GetFromPool(poolKey, position, Quaternion.identity)
            : Instantiate(fallbackPrefab, position, Quaternion.identity);

        if (go != null && autoReturn)
        {
            var token = go.GetComponent<SkillPoolToken>();
            if (token != null) token.ReturnToPoolAfterDelay(impactLifetime);
        }

        return go;
    }

    private void Release()
    {
        if (_ownerSkill != null) { _ownerSkill.ReleaseSkillObject(gameObject); return; }
        if (SkillObjectPolling.Instance != null) { SkillObjectPolling.Instance.ReturnByInstance(gameObject); return; }
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _didHit = false;
        _target = null;
        _targetMove = null;
        audioSource?.Stop();
        ResetRigidbody();
    }

    private void ConfigurePhysics()
    {
        if (_cachedColliders == null || _cachedColliders.Length == 0)
            _cachedColliders = GetComponentsInChildren<Collider>(true);
        foreach (var col in _cachedColliders)
            if (col != null) col.isTrigger = true;

        if (_cachedRb == null) _cachedRb = GetComponent<Rigidbody>();
        if (_cachedRb != null)
        {
            _cachedRb.isKinematic = true;
            _cachedRb.useGravity = false;
        }
    }

    private void ResetRigidbody()
    {
        if (_cachedRb == null) return;
        _cachedRb.linearVelocity = Vector3.zero;
        _cachedRb.angularVelocity = Vector3.zero;
    }

    private static double ResolveStat(PokemonData data, int level, string statName)
    {
        if (data != null && data.TryGetStatValueByLevel(level, statName, out double val)) return val;
        Debug.LogWarning($"[LunelleSkill] Missing stat '{statName}' at level {level}");
        return 0;
    }

    private void LogDebug(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[LunelleSkill:{name}] {message}", this);
    }
}
