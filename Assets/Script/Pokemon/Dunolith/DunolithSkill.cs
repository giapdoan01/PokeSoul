using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class DunolithSkill : MonoBehaviour, IPokemonSkillLaunch
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog;

    [Header("Skill Settings")]
    [SerializeField] private float preCastDelay  = 0.75f;
    [SerializeField] private float preCastHeight = 2f;
    [SerializeField] private float descendSpeed  = 5f;
    [SerializeField] private float stunDuration  = 1f;

    [Header("VFX")]
    [SerializeField] private GameObject preCastVFXPrefab;
    [SerializeField] private GameObject afterCastVFXPrefab;
    [SerializeField] private int vfxPreloadAmount = 3;

    [Header("SFX")]
    public AudioClip preCastSFX;
    public AudioClip impactSFX;
    public AudioClip afterCastSFX;
    public AudioSource audioSource;

    private const string PreCastPoolKey  = "DunolithPreCast";
    private const string AfterCastPoolKey = "DunolithAfterCast";

    private PokemonSkill ownerSkill;
    private Transform _target;
    private EnemyMoveController _targetMove;
    private double _mainDamage;
    private double _stunDamage;
    private bool _didHit;
    private bool _descending;

    private Collider[] _colliders;
    private Rigidbody _rb;
    private CancellationTokenSource _cts;

    // ── Lifecycle ──

    private void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>(true);
        _rb = GetComponent<Rigidbody>();
        RegisterPools();
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _didHit      = false;
        _descending  = false;
        _target      = null;
        _targetMove  = null;
        ResetRigidbody();
    }

    // ── IPokemonSkillLaunch ──

    public void Launch(PokemonSkill owner, Transform targetEnemy, PokemonData pokemonData, int level, string attack)
    {
        ownerSkill  = owner;
        _target     = targetEnemy;
        _targetMove = targetEnemy?.GetComponent<EnemyMoveController>();
        _mainDamage = ResolveStat(pokemonData, level, "damage");
        _stunDamage = ResolveStat(pokemonData, level, "stundamage");
        _didHit     = false;
        _descending = false;

        // Disable colliders until descending phase
        SetCollidersEnabled(false);
        gameObject.SetActive(true);

        audioSource?.PlayOneShot(preCastSFX);

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        PreCastPhaseAsync(_cts.Token).Forget();

        LogDebug($"Launch. target={_target?.name}, mainDmg={_mainDamage}, stunDmg={_stunDamage}");
    }

    // ── Phase 1: Pre-cast delay ──

    private async UniTaskVoid PreCastPhaseAsync(CancellationToken ct)
    {
        if (_target == null) { Release(); return; }

        // Spawn above target
        Vector3 preCastPos = _target.position + Vector3.up * preCastHeight;
        transform.position = preCastPos;

        var preCastVFX = GetFromPool(PreCastPoolKey, preCastVFXPrefab, preCastPos);

        await UniTask.Delay((int)(preCastDelay * 1000), cancellationToken: ct);

        ReturnToPool(preCastVFX, PreCastPoolKey);
        SetCollidersEnabled(true);
        _descending = true;

        LogDebug("Pre-cast done. Descending.");
    }

    // ── Phase 2: Descend toward target ──

    private void Update()
    {
        if (!_descending || _didHit) return;

        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            Release();
            return;
        }

        Vector3 targetPos = _targetMove != null ? _targetMove.FootPosition : _target.position;
        Vector3 direction = targetPos - transform.position;

        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.position += direction.normalized * descendSpeed * Time.deltaTime;
            transform.forward   = direction.normalized;
        }
    }

    // ── Phase 3: Hit ──

    private void OnTriggerEnter(Collider other)
    {
        if (_didHit || !_descending) return;
        if (!IsTargetCollision(other)) return;

        _didHit     = true;
        _descending = false;

        var hp     = other.GetComponentInParent<EnemyHPController>() ?? other.GetComponent<EnemyHPController>();
        var status = other.GetComponentInParent<EnemyStatusEffects>() ?? other.GetComponent<EnemyStatusEffects>();

        // Main damage
        hp?.TakeDamage(_mainDamage);
        MonImpactSoundManager.Instance?.PlaySound(impactSFX);


        // Phase 4: Stun + stun damage
        Vector3 hitPos = _target != null ? _target.position : transform.position;
        AfterCastEffectAsync(hp, status, hitPos).Forget();

        Release();
        LogDebug($"Hit. mainDmg={_mainDamage}");
    }

    // ── Phase 4: After-cast stun effect ──

    private async UniTaskVoid AfterCastEffectAsync(EnemyHPController hp, EnemyStatusEffects status, Vector3 position)
    {
        var afterCastVFX = GetFromPool(AfterCastPoolKey, afterCastVFXPrefab, position);
        MonImpactSoundManager.Instance?.PlaySound(afterCastSFX);

        status?.Stun(stunDuration);

        await UniTask.Delay((int)(stunDuration * 1000));

        if (hp != null && hp.gameObject.activeInHierarchy)
        {
            hp.TakeDamage(_stunDamage);
            LogDebug($"Stun damage applied: {_stunDamage}");
        }

        ReturnToPool(afterCastVFX, AfterCastPoolKey);
    }

    // ── Helpers ──

    private bool IsTargetCollision(Collider other)
    {
        if (other == null) return false;
        Transform t = other.transform;
        if (_target != null && (t == _target || t.IsChildOf(_target) || _target.IsChildOf(t)))
            return true;
        return ownerSkill != null && other.CompareTag(ownerSkill.EnemyTag);
    }

    private void SetCollidersEnabled(bool active)
    {
        if (_colliders == null) _colliders = GetComponentsInChildren<Collider>(true);
        foreach (var col in _colliders)
        {
            col.isTrigger = true;
            col.enabled   = active;
        }
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity  = false;
        }
    }

    private void ResetRigidbody()
    {
        if (_rb == null) return;
        _rb.linearVelocity  = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    private void RegisterPools()
    {
        if (SkillObjectPolling.Instance == null) return;
        if (preCastVFXPrefab != null)
            SkillObjectPolling.Instance.RegisterPrefab(PreCastPoolKey, preCastVFXPrefab, vfxPreloadAmount);
        if (afterCastVFXPrefab != null)
            SkillObjectPolling.Instance.RegisterPrefab(AfterCastPoolKey, afterCastVFXPrefab, vfxPreloadAmount);
    }

    private GameObject GetFromPool(string key, GameObject fallbackPrefab, Vector3 position)
    {
        if (SkillObjectPolling.Instance != null)
        {
            var go = SkillObjectPolling.Instance.GetFromPool(key, position, Quaternion.identity);
            if (go != null) return go;
        }
        return fallbackPrefab != null ? Instantiate(fallbackPrefab, position, Quaternion.identity) : null;
    }

    private static void ReturnToPool(GameObject go, string key)
    {
        if (go == null) return;
        if (SkillObjectPolling.Instance != null)
            SkillObjectPolling.Instance.ReturnByInstance(go);
        else
            Destroy(go);
    }

    private void Release()
    {
        _cts?.Cancel();
        if (ownerSkill != null) { ownerSkill.ReleaseSkillObject(gameObject); return; }
        if (SkillObjectPolling.Instance != null) { SkillObjectPolling.Instance.ReturnByInstance(gameObject); return; }
        Destroy(gameObject);
    }

    private static double ResolveStat(PokemonData data, int level, string statName)
    {
        if (data != null && data.TryGetStatValueByLevel(level, statName, out double val)) return val;
        Debug.LogWarning($"[DunolithSkill] Missing stat '{statName}' at level {level}");
        return 0;
    }

    private void LogDebug(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[DunolithSkill:{name}] {message}", this);
    }
}
