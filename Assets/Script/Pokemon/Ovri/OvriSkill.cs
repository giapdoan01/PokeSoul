using UnityEngine;

public class OvriSkill : MonoBehaviour, IPokemonSkillLaunch
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog;

    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float projectileSpread = 0.35f;
    [SerializeField] private GameObject ovriImpact;
    [SerializeField] private float impactLifeTime = 1.5f;
    [SerializeField] private float impactHeightOffset = 1f;
    [SerializeField] private int impactPreloadAmount = 5;

    private const string ImpactPoolKey = "OvriImpact";

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

    private void Awake()
    {
        cachedColliders = GetComponentsInChildren<Collider>(true);
        cachedRigidbody = GetComponent<Rigidbody>();

        ConfigurePhysicsForTriggerOnly();
        RegisterImpactPool();
    }

    private void RegisterImpactPool()
    {
        if (ovriImpact == null || SkillObjectPolling.Instance == null) return;
        SkillObjectPolling.Instance.RegisterPrefab(ImpactPoolKey, ovriImpact, impactPreloadAmount);
    }

    public void Launch(PokemonSkill owner, Transform targetEnemy, PokemonData pokemonData, int level, string attack)
    {
        LaunchInternal(owner, targetEnemy, pokemonData, level, attack, true);
    }

    private void LaunchInternal(PokemonSkill owner, Transform targetEnemy, PokemonData pokemonData, int level, string attack, bool spawnBurst)
    {
        ownerSkill = owner;
        target = targetEnemy;
        runtimeDamage = ResolveDamageByLevel(pokemonData, level);
        didHit = false;
        ConfigurePhysicsForTriggerOnly();

        if (spawnBurst)
        {
            SpawnExtraProjectiles(pokemonData, level, targetEnemy, attack);
            if (skillSFX != null && audioSource != null)
                audioSource.PlayOneShot(skillSFX);
        }

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (didHit) return;
        if (target == null || !target.gameObject.activeInHierarchy) { Release(); return; }

        Vector3 direction = target.position - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDir = direction.normalized;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
            transform.forward = moveDir;
        }
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
        Transform t = other.transform;
        if (target != null && (t == target || t.IsChildOf(target) || target.IsChildOf(t)))
            return true;
        return ownerSkill != null && other.CompareTag(ownerSkill.EnemyTag);
    }

    private double ResolveDamageByLevel(PokemonData pokemonData, int level)
    {
        if (pokemonData != null && pokemonData.TryGetStatValueByLevel(level, "damage", out double damage))
            return damage;
        Debug.LogWarning("[OvriSkill] Missing 'damage' stat. Damage = 0.");
        return 0;
    }

    private int ResolveProjectileCount(PokemonData pokemonData, int level)
    {
        if (pokemonData != null && pokemonData.TryGetStatValueByLevel(level, "pc", out double pc))
            return Mathf.Max(1, Mathf.RoundToInt((float)pc));
        return 1;
    }

    private void SpawnExtraProjectiles(PokemonData pokemonData, int level, Transform targetEnemy, string attack)
    {
        if (ownerSkill == null) return;
        int projectileCount = ResolveProjectileCount(pokemonData, level);
        if (projectileCount <= 1) return;

        for (int i = 1; i < projectileCount; i++)
        {
            Vector3 spawnPosition = GetProjectileSpawnPosition(i, projectileCount);
            Quaternion spawnRotation = GetProjectileRotation(spawnPosition, targetEnemy);
            GameObject extra = ownerSkill.GetSkillObjectFromPool(spawnPosition, spawnRotation);
            if (extra == null) continue;

            OvriSkill extraSkill = extra.GetComponent<OvriSkill>();
            if (extraSkill == null) { ownerSkill.ReleaseSkillObject(extra); continue; }

            extraSkill.LaunchInternal(ownerSkill, targetEnemy, pokemonData, level, attack, false);
        }
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

    private void HandleHit(Collider other, Vector3? hitPoint)
    {
        if (didHit) return;
        if (!IsTargetCollision(other)) return;
        didHit = true;

        EnemyHPController hp = other.GetComponentInParent<EnemyHPController>() ?? other.GetComponent<EnemyHPController>();
        if (hp != null) hp.TakeDamage(runtimeDamage);

        SpawnImpactVfx(other, hitPoint);
        if (impactSFX != null) MonImpactSoundManager.Instance?.PlaySound(impactSFX);
        Release();
    }

    private void SpawnImpactVfx(Collider hitCollider) => SpawnImpactVfx(hitCollider, null);

    private void SpawnImpactVfx(Collider hitCollider, Vector3? overridePosition)
    {
        if (ovriImpact == null) return;
        Vector3 pos = (overridePosition ?? (hitCollider != null ? hitCollider.ClosestPoint(transform.position) : transform.position))
            + Vector3.up * impactHeightOffset;

        if (SkillObjectPolling.Instance != null)
        {
            RegisterImpactPool();
            GameObject impact = SkillObjectPolling.Instance.GetFromPool(ImpactPoolKey, pos, Quaternion.identity);
            if (impact != null)
            {
                SkillPoolToken token = impact.GetComponent<SkillPoolToken>();
                if (token != null) token.ReturnToPoolAfterDelay(impactLifeTime);
                else SkillObjectPolling.Instance.ReturnByInstance(impact);
                return;
            }
        }
        GameObject fallback = Instantiate(ovriImpact, pos, Quaternion.identity);
        if (impactLifeTime > 0f) Destroy(fallback, impactLifeTime);
    }

    private void Release()
    {
        if (ownerSkill != null) { ownerSkill.ReleaseSkillObject(gameObject); return; }
        if (SkillObjectPolling.Instance != null) { SkillObjectPolling.Instance.ReturnByInstance(gameObject); return; }
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        didHit = false;
        target = null;
        audioSource?.Stop();
        ResetRigidbodyState();
    }

    private void ConfigurePhysicsForTriggerOnly()
    {
        if (cachedColliders == null || cachedColliders.Length == 0)
            cachedColliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cachedColliders.Length; i++)
            if (cachedColliders[i] != null) cachedColliders[i].isTrigger = true;

        if (cachedRigidbody == null) cachedRigidbody = GetComponent<Rigidbody>();
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
        Debug.Log($"[OvriSkill:{name}] {message}", this);
    }
}
