using UnityEngine;
using System.Collections.Generic;

public class HomaryuSkill : MonoBehaviour, IPokemonSkillLaunch
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog;

    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float projectileSpread = 0.35f;
    [SerializeField] private GameObject homaryuImpact;
    [SerializeField] private float impactLifeTime = 1.5f;
    [SerializeField] private float impactHeightOffset = 1f;
    [SerializeField] private int impactPreloadAmount = 5;

    [SerializeField] private float launchHeightOffset = 1f;
    [SerializeField] private float curveWidth = 3f;
    [SerializeField] private float curveHeight = 1.5f;

    private const string ImpactPoolKey = "HomaryuImpact";

    [Header("SFX")]
    public AudioClip skillSFX;
    public AudioClip impactSFX;
    public AudioSource audioSource;

    private PokemonSkill ownerSkill;
    private Transform target;
    private double runtimeDamage;
    private bool didHit;
    private Collider[] cachedColliders;
    private Rigidbody cachedRigidbody;

    // Bezier curve state
    private bool useCurve;
    private Vector3 bezierStart;
    private Vector3 bezierControl;
    private float bezierT;
    private float bezierDuration;

    private void Awake()
    {
        cachedColliders = GetComponentsInChildren<Collider>(true);
        cachedRigidbody = GetComponent<Rigidbody>();

        ConfigurePhysicsForTriggerOnly();
        RegisterImpactPool();
    }

    private void RegisterImpactPool()
    {
        if (homaryuImpact == null || SkillObjectPolling.Instance == null) return;

        SkillObjectPolling.Instance.RegisterPrefab(ImpactPoolKey, homaryuImpact, impactPreloadAmount);
        LogDebug($"Impact pool registered. key={ImpactPoolKey}, preload={impactPreloadAmount}");
    }

    public void Launch(PokemonSkill owner, Transform targetEnemy, PokemonData pokemonData, int level, string attack)
    {
        LogDebug($"Launch called. owner={(owner != null ? owner.name : "null")}, target={(targetEnemy != null ? targetEnemy.name : "null")}, level={level}, attack={attack}");
        LaunchInternal(owner, targetEnemy, pokemonData, level, attack, true, 1f);
    }

    private void LaunchInternal(PokemonSkill owner, Transform targetEnemy, PokemonData pokemonData, int level, string attack, bool spawnBurst, float curveSign = 0f)
    {
        ownerSkill = owner;
        target = targetEnemy;
        runtimeDamage = ResolveDamageByLevel(pokemonData, level);
        didHit = false;
        ConfigurePhysicsForTriggerOnly();
        transform.position += Vector3.up * launchHeightOffset;

        useCurve = curveSign != 0f && targetEnemy != null;
        if (useCurve)
            SetupBezier(targetEnemy, curveSign);

        if (spawnBurst)
        {
            SpawnExtraProjectiles(pokemonData, level, targetEnemy, attack);
            if (skillSFX != null && audioSource != null)
                audioSource.PlayOneShot(skillSFX);
        }

        gameObject.SetActive(true);
        LogDebug($"LaunchInternal start. damage={runtimeDamage}, curveSign={curveSign}");
    }

    private void SetupBezier(Transform targetEnemy, float curveSign)
    {
        bezierStart = transform.position;
        Vector3 end = targetEnemy.position;
        Vector3 mid = (bezierStart + end) * 0.5f;

        Vector3 toTarget = (end - bezierStart);
        toTarget.y = 0f;
        toTarget = toTarget.normalized;

        // Perpendicular luôn nằm ngang trên mặt phẳng XZ, không phụ thuộc hướng bay
        Vector3 right = new Vector3(-toTarget.z, 0f, toTarget.x);

        bezierControl = mid + right * curveSign * curveWidth + Vector3.up * curveHeight;
        bezierT = 0f;
        float distance = Vector3.Distance(bezierStart, end);
        bezierDuration = distance / moveSpeed;
    }

    private void Update()
    {
        if (didHit) return;

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            LogDebug("Target missing or inactive. Release projectile.");
            Release();
            return;
        }

        if (useCurve)
            MoveBezier();
        else
            MoveStraight();
    }

    private void MoveStraight()
    {
        Vector3 direction = target.position - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDir = direction.normalized;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
            transform.forward = moveDir;
        }
    }

    private void MoveBezier()
    {
        if (bezierDuration <= 0f)
        {
            MoveStraight();
            return;
        }

        bezierT += Time.deltaTime / bezierDuration;

        if (bezierT >= 1f)
        {
            bezierT = 1f;
            useCurve = false;
        }

        float t = bezierT;
        Vector3 end = target.position;
        Vector3 prev = transform.position;
        Vector3 next = (1 - t) * (1 - t) * bezierStart + 2 * (1 - t) * t * bezierControl + t * t * end;

        Vector3 moveDir = next - prev;
        if (moveDir.sqrMagnitude > 0.0001f)
            transform.forward = moveDir.normalized;

        transform.position = next;
    }

    private void OnTriggerEnter(Collider other) => HandleHit(other, null);

    private void OnCollisionEnter(Collision collision)
    {
        Collider hitCollider = collision != null ? collision.collider : null;
        Vector3? hitPoint = null;

        if (collision != null && collision.contactCount > 0)
            hitPoint = collision.GetContact(0).point;

        HandleHit(hitCollider, hitPoint);
    }

    private bool IsTargetCollision(Collider other)
    {
        if (other == null) return false;

        Transform otherTransform = other.transform;

        if (target != null && (otherTransform == target || otherTransform.IsChildOf(target) || target.IsChildOf(otherTransform)))
            return true;

        if (ownerSkill != null && other.CompareTag(ownerSkill.EnemyTag))
            return true;

        return false;
    }

    private double ResolveDamageByLevel(PokemonData pokemonData, int level)
    {
        if (pokemonData != null && pokemonData.TryGetStatValueByLevel(level, "damage", out double damage))
            return damage;

        Debug.LogWarning("[HomaryuSkill] Missing 'damage' stat in PokemonData level. Damage = 0.");
        return 0;
    }

    private int ResolveProjectileCount(PokemonData pokemonData, int level)
    {
        if (pokemonData != null && pokemonData.TryGetStatValueByLevel(level, "pc", out double projectileCount))
            return Mathf.Max(1, Mathf.RoundToInt((float)projectileCount));

        return 1;
    }

    private void SpawnExtraProjectiles(PokemonData pokemonData, int level, Transform primaryTarget, string attack)
    {
        if (ownerSkill == null) return;

        int projectileCount = ResolveProjectileCount(pokemonData, level);
        LogDebug($"Projectile count resolved: {projectileCount}");
        if (projectileCount <= 1) return;

        Transform[] targets = FindNearestTargets(projectileCount);

        for (int i = 1; i < projectileCount; i++)
        {
            bool hasDifferentTarget = i < targets.Length && targets[i] != primaryTarget;
            Transform extraTarget = hasDifferentTarget ? targets[i] : primaryTarget;

            // Đạn đầu = +1f, đạn tiếp theo xen kẽ -1f, +1f, -1f...
            float curveSign = hasDifferentTarget ? 0f : (i % 2 == 1 ? -1f : 1f);

            Vector3 spawnPosition = GetProjectileSpawnPosition(i, projectileCount);
            Quaternion spawnRotation = GetProjectileRotation(spawnPosition, extraTarget);
            GameObject extraProjectile = ownerSkill.GetSkillObjectFromPool(spawnPosition, spawnRotation);
            if (extraProjectile == null)
            {
                LogDebug($"Extra projectile spawn failed at index={i}");
                continue;
            }

            HomaryuSkill extraSkill = extraProjectile.GetComponent<HomaryuSkill>();
            if (extraSkill == null)
            {
                ownerSkill.ReleaseSkillObject(extraProjectile);
                continue;
            }

            extraSkill.LaunchInternal(ownerSkill, extraTarget, pokemonData, level, attack, false, curveSign);
            LogDebug($"Extra projectile spawned: {extraProjectile.name} index={i}, target={extraTarget?.name}, curveSign={curveSign}");
        }
    }

    private Transform[] FindNearestTargets(int count)
    {
        if (ownerSkill == null || count <= 0 || EnemyRegistry.Instance == null)
            return System.Array.Empty<Transform>();

        var targets = EnemyRegistry.Instance.GetTargetsInRange(ownerSkill.CastPoint.position, ownerSkill.AttackRange, count);
        return targets.ToArray();
    }

    private Vector3 GetProjectileSpawnPosition(int index, int totalCount)
    {
        Vector3 origin = ownerSkill != null ? ownerSkill.CastPoint.position : transform.position;
        if (totalCount <= 1 || ownerSkill == null) return origin;

        float centeredIndex = index - (totalCount - 1) * 0.5f;
        return origin + ownerSkill.CastPoint.right * centeredIndex * projectileSpread;
    }

    private Quaternion GetProjectileRotation(Vector3 spawnPosition, Transform targetEnemy)
    {
        if (ownerSkill == null) return transform.rotation;
        if (targetEnemy == null) return ownerSkill.CastPoint.rotation;

        Vector3 direction = targetEnemy.position - spawnPosition;
        if (direction.sqrMagnitude <= 0.001f) return ownerSkill.CastPoint.rotation;

        return Quaternion.LookRotation(direction.normalized);
    }

    private void SpawnImpactVfx(Collider hitCollider) => SpawnImpactVfx(hitCollider, null);

    private void SpawnImpactVfx(Collider hitCollider, Vector3? overridePosition)
    {
        if (homaryuImpact == null)
        {
            LogDebug("homaryuImpact is null, skip impact VFX.");
            return;
        }

        Vector3 spawnPosition = overridePosition ?? (hitCollider != null ? hitCollider.ClosestPoint(transform.position) : transform.position);
        spawnPosition += Vector3.up * impactHeightOffset;

        if (SkillObjectPolling.Instance != null)
        {
            RegisterImpactPool();
            GameObject impact = SkillObjectPolling.Instance.GetFromPool(ImpactPoolKey, spawnPosition, Quaternion.identity);
            if (impact != null)
            {
                LogDebug($"Impact VFX from pool: {impact.name}");
                if (impactLifeTime > 0f)
                {
                    SkillPoolToken token = impact.GetComponent<SkillPoolToken>();
                    if (token != null)
                        token.ReturnToPoolAfterDelay(impactLifeTime);
                    else
                        SkillObjectPolling.Instance.ReturnByInstance(impact);
                }
                return;
            }
        }

        GameObject fallback = Instantiate(homaryuImpact, spawnPosition, Quaternion.identity);
        LogDebug($"Impact VFX fallback instantiate: {fallback.name}");
        if (impactLifeTime > 0f)
            Destroy(fallback, impactLifeTime);
    }

    private void Release()
    {
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
        didHit = false;
        useCurve = false;
        bezierT = 0f;
        target = null;
        audioSource?.Stop();
        ResetRigidbodyState();
    }

    private void HandleHit(Collider other, Vector3? hitPoint)
    {
        if (didHit) return;
        if (!IsTargetCollision(other)) return;

        didHit = true;
        LogDebug($"Hit collider: {other.name}");

        EnemyHPController enemyHp = other.GetComponentInParent<EnemyHPController>();
        if (enemyHp == null)
            enemyHp = other.GetComponent<EnemyHPController>();

        if (enemyHp != null)
        {
            enemyHp.TakeDamage(runtimeDamage);
            LogDebug($"Damage applied: {runtimeDamage} to {enemyHp.name}");
        }
        else
        {
            LogDebug("EnemyHPController not found on hit target.");
        }

        SpawnImpactVfx(other, hitPoint);
        if (impactSFX != null)
            MonImpactSoundManager.Instance?.PlaySound(impactSFX);
        Release();
    }

    private void ConfigurePhysicsForTriggerOnly()
    {
        if (cachedColliders == null || cachedColliders.Length == 0)
            cachedColliders = GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < cachedColliders.Length; i++)
        {
            if (cachedColliders[i] != null)
                cachedColliders[i].isTrigger = true;
        }

        if (cachedRigidbody == null)
            cachedRigidbody = GetComponent<Rigidbody>();

        if (cachedRigidbody != null)
        {
            cachedRigidbody.isKinematic = true;
            cachedRigidbody.useGravity = false;
            cachedRigidbody.linearVelocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void ResetRigidbodyState()
    {
        if (cachedRigidbody == null) return;

        cachedRigidbody.linearVelocity = Vector3.zero;
        cachedRigidbody.angularVelocity = Vector3.zero;
    }

    private void LogDebug(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[HomaryuSkill:{name}] {message}", this);
    }
}
