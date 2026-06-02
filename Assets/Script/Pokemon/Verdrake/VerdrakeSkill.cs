using UnityEngine;
using System.Collections.Generic;

public class VerdrakeSkill : MonoBehaviour, IPokemonSkillLaunch
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog;

    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float skillHeightOffset = 2f;
    [SerializeField] private float hitRadius = 0.4f;
    [SerializeField] private GameObject targetVfxPrefab;
    [SerializeField] private GameObject verdrakeImpactPrefab;
    [SerializeField] private float impactLifeTime = 1.5f;
    [SerializeField] private int vfxPreloadAmount = 5;
    [SerializeField] private int impactPreloadAmount = 5;

    private const string TargetVfxPoolKey = "VerdrakeTargetVfx";
    private const string ImpactPoolKey = "VerdrakeImpact";

    [Header("SFX")]
    public AudioClip skillSFX;
    public AudioClip impactSFX;
    public AudioSource audioSource;

    private PokemonSkill ownerSkill;
    private Transform targetEnemy;
    private EnemyMoveController targetMoveController;
    private double runtimeDamage;
    private bool didDamage;
    private bool didFinish;

    private GameObject activeTargetVfx;
    private bool targetVfxReleased;
    private Vector3 lastKnownVfxPosition;
    private Collider[] cachedColliders;

    private void Awake()
    {
        cachedColliders = GetComponentsInChildren<Collider>(true);
        ConfigureColliders();
        RegisterPools();
    }

    private void RegisterPools()
    {
        if (SkillObjectPolling.Instance == null) return;

        if (targetVfxPrefab != null)
            SkillObjectPolling.Instance.RegisterPrefab(TargetVfxPoolKey, targetVfxPrefab, vfxPreloadAmount);

        if (verdrakeImpactPrefab != null)
            SkillObjectPolling.Instance.RegisterPrefab(ImpactPoolKey, verdrakeImpactPrefab, impactPreloadAmount);
    }

    public void Launch(PokemonSkill owner, Transform enemy, PokemonData pokemonData, int level, string attack)
    {
        LogDebug($"Launch called. target={(enemy != null ? enemy.name : "null")}");
        LaunchInternal(owner, enemy, pokemonData, level, attack, true);
    }

    private void LaunchInternal(PokemonSkill owner, Transform enemy, PokemonData pokemonData, int level, string attack, bool spawnExtra)
    {
        ownerSkill = owner;
        targetEnemy = enemy;
        targetMoveController = enemy != null ? enemy.GetComponent<EnemyMoveController>() : null;
        runtimeDamage = ResolveDamage(pokemonData, level);
        didDamage = false;
        didFinish = false;
        targetVfxReleased = false;

        ConfigureColliders();

        // Spawn trên cao, mặt Z chỉ xuống dưới
        transform.position = targetEnemy.position + Vector3.up * skillHeightOffset;
        transform.forward = Vector3.down;
        lastKnownVfxPosition = targetEnemy.position;

        SpawnTargetVfx();

        if (spawnExtra)
        {
            SpawnExtraSkills(pokemonData, level, attack);
            if (skillSFX != null && audioSource != null)
                audioSource.PlayOneShot(skillSFX);
        }

        gameObject.SetActive(true);
        LogDebug($"LaunchInternal done. damage={runtimeDamage}");
    }

    private void Update()
    {
        if (didFinish) return;

        bool enemyAlive = targetEnemy != null && targetEnemy.gameObject.activeInHierarchy;

        if (enemyAlive)
        {
            Vector3 footPos = targetMoveController != null
                ? targetMoveController.FootPosition
                : targetEnemy.position;

            if (!targetVfxReleased && activeTargetVfx != null)
            {
                activeTargetVfx.transform.position = footPos;
                lastKnownVfxPosition = footPos;
            }
        }

        Vector3 target = activeTargetVfx != null ? activeTargetVfx.transform.position : lastKnownVfxPosition;
        Vector3 direction = target - transform.position;

        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            transform.forward = direction.normalized;
        }

        // Distance check thay vì OnTriggerEnter để tránh physics timing issue
        if (Vector3.Distance(transform.position, target) <= hitRadius)
        {
            LogDebug("Reached targetVfx. Finish.");
            if (!didDamage)
                TryApplyDamageAtPosition(target);
            SpawnImpact(target);
            if (impactSFX != null)
                MonImpactSoundManager.Instance?.PlaySound(impactSFX);
            ReleaseTargetVfx();
            Release();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (didFinish) return;

        // Chỉ xử lý damage khi chạm enemy, không dùng trigger để detect VFX
        if (!didDamage && IsEnemyCollision(other))
        {
            TryApplyDamage(other);
        }
    }

    private bool IsEnemyCollision(Collider other)
    {
        if (other == null) return false;

        Transform t = other.transform;
        if (targetEnemy != null &&
            (t == targetEnemy || t.IsChildOf(targetEnemy) || targetEnemy.IsChildOf(t)))
            return true;

        return ownerSkill != null && other.CompareTag(ownerSkill.EnemyTag);
    }

    private void TryApplyDamage(Collider other)
    {
        EnemyHPController hp = other.GetComponentInParent<EnemyHPController>() ?? other.GetComponent<EnemyHPController>();
        if (hp != null)
        {
            hp.TakeDamage(runtimeDamage);
            didDamage = true;
            LogDebug($"Damage applied: {runtimeDamage}");
        }
    }

    private void TryApplyDamageAtPosition(Vector3 position)
    {
        if (didDamage) return;

        // Fallback damage nếu enemy chưa bị hit khi skill tới đích
        bool enemyAlive = targetEnemy != null && targetEnemy.gameObject.activeInHierarchy;
        if (!enemyAlive) return;

        EnemyHPController hp = targetEnemy.GetComponent<EnemyHPController>()
            ?? targetEnemy.GetComponentInChildren<EnemyHPController>();
        if (hp != null)
        {
            hp.TakeDamage(runtimeDamage);
            didDamage = true;
            LogDebug($"Damage applied at position: {runtimeDamage}");
        }
    }

    private void SpawnTargetVfx()
    {
        if (targetVfxPrefab == null || targetEnemy == null) return;

        Vector3 footPos = targetMoveController != null
            ? targetMoveController.FootPosition
            : targetEnemy.position;

        RegisterPools();

        if (SkillObjectPolling.Instance != null)
        {
            activeTargetVfx = SkillObjectPolling.Instance.GetFromPool(TargetVfxPoolKey, footPos, Quaternion.identity);
        }
        else
        {
            activeTargetVfx = Instantiate(targetVfxPrefab, footPos, Quaternion.identity);
        }

        if (activeTargetVfx != null)
        {
            foreach (Collider col in activeTargetVfx.GetComponentsInChildren<Collider>(true))
                col.isTrigger = true;

            // Cần Rigidbody để OnTriggerEnter fire giữa 2 trigger collider
            Rigidbody rb = activeTargetVfx.GetComponent<Rigidbody>()
                ?? activeTargetVfx.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            VerdrakeTargetVfxMarker marker = activeTargetVfx.GetComponent<VerdrakeTargetVfxMarker>()
                ?? activeTargetVfx.AddComponent<VerdrakeTargetVfxMarker>();
            marker.Owner = this;
        }

        LogDebug($"TargetVfx spawned at {footPos}");
    }

    private void ReleaseTargetVfx()
    {
        if (targetVfxReleased || activeTargetVfx == null) return;

        targetVfxReleased = true;

        if (SkillObjectPolling.Instance != null)
            SkillObjectPolling.Instance.ReturnByInstance(activeTargetVfx);
        else
            Destroy(activeTargetVfx);

        activeTargetVfx = null;
    }

    private void SpawnImpact(Vector3 position)
    {
        if (verdrakeImpactPrefab == null) return;

        RegisterPools();

        if (SkillObjectPolling.Instance != null)
        {
            GameObject impact = SkillObjectPolling.Instance.GetFromPool(ImpactPoolKey, position, Quaternion.identity);
            if (impact != null)
            {
                SkillPoolToken token = impact.GetComponent<SkillPoolToken>();
                if (token != null)
                    token.ReturnToPoolAfterDelay(impactLifeTime);
                return;
            }
        }

        GameObject fallback = Instantiate(verdrakeImpactPrefab, position, Quaternion.identity);
        if (impactLifeTime > 0f)
            Destroy(fallback, impactLifeTime);
    }

    private void SpawnExtraSkills(PokemonData pokemonData, int level, string attack)
    {
        if (ownerSkill == null) return;

        int targetCount = ResolveTargetCount(pokemonData, level);
        if (targetCount <= 1) return;

        Transform[] targets = FindPriorityTargets(targetCount);

        for (int i = 1; i < targets.Length; i++)
        {
            if (targets[i] == null || targets[i] == targetEnemy) continue;

            Vector3 spawnPos = targets[i].position + Vector3.up * skillHeightOffset;
            GameObject extraObj = ownerSkill.GetSkillObjectFromPool(spawnPos, Quaternion.identity);
            if (extraObj == null) continue;

            VerdrakeSkill extra = extraObj.GetComponent<VerdrakeSkill>();
            if (extra == null)
            {
                ownerSkill.ReleaseSkillObject(extraObj);
                continue;
            }

            extra.LaunchInternal(ownerSkill, targets[i], pokemonData, level, attack, false);
            LogDebug($"Extra skill spawned on {targets[i].name}");
        }
    }

    private Transform[] FindPriorityTargets(int count)
    {
        if (ownerSkill == null || EnemyRegistry.Instance == null)
            return System.Array.Empty<Transform>();

        var targets = EnemyRegistry.Instance.GetTargetsInRange(ownerSkill.CastPoint.position, ownerSkill.AttackRange, count);
        return targets.ToArray();
    }

    private double ResolveDamage(PokemonData pokemonData, int level)
    {
        if (pokemonData != null && pokemonData.TryGetStatValueByLevel(level, "damage", out double damage))
            return damage;

        Debug.LogWarning("[VerdrakeSkill] Missing 'damage' stat. Damage = 0.");
        return 0;
    }

    private int ResolveTargetCount(PokemonData pokemonData, int level)
    {
        if (pokemonData != null && pokemonData.TryGetStatValueByLevel(level, "targetcount", out double tc))
            return Mathf.Max(1, Mathf.RoundToInt((float)tc));

        return 1;
    }

    private void Release()
    {
        didFinish = true;

        if (ownerSkill != null)
        {
            ownerSkill.ReleaseSkillObject(gameObject);
            return;
        }

        if (SkillObjectPolling.Instance != null)
        {
            SkillObjectPolling.Instance.ReturnByInstance(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    private void OnDisable()
    {
        didDamage = false;
        didFinish = false;
        targetVfxReleased = false;
        targetEnemy = null;
        targetMoveController = null;
        activeTargetVfx = null;
        audioSource?.Stop();
    }

    private void ConfigureColliders()
    {
        if (cachedColliders == null || cachedColliders.Length == 0)
            cachedColliders = GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < cachedColliders.Length; i++)
        {
            if (cachedColliders[i] != null)
                cachedColliders[i].isTrigger = true;
        }
    }

    private void LogDebug(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[VerdrakeSkill:{name}] {message}", this);
    }
}

public class VerdrakeTargetVfxMarker : MonoBehaviour
{
    public VerdrakeSkill Owner { get; set; }
}
