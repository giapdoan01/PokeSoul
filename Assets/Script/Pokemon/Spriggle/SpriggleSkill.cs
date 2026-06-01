using UnityEngine;

public class SpriggleSkill : MonoBehaviour, IPokemonSkillLaunch
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog;

    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private GameObject spriggleImpact;
    [SerializeField] private float impactLifeTime = 1.5f;
    [SerializeField] private float impactHeightOffset = 1f;
    [SerializeField] private int impactPreloadAmount = 5;

    private const string ImpactPoolKey = "SpriggleImpact";

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
        if (spriggleImpact == null || SkillObjectPolling.Instance == null)
        {
            return;
        }

        SkillObjectPolling.Instance.RegisterPrefab(ImpactPoolKey, spriggleImpact, impactPreloadAmount);
    }

    public void Launch(PokemonSkill owner, Transform targetEnemy, PokemonData pokemonData, int level, string attack)
    {
        ownerSkill = owner;
        target = targetEnemy;
        runtimeDamage = ResolveDamage(pokemonData, level);
        didHit = false;
        ConfigurePhysicsForTriggerOnly();
        if (skillSFX != null && audioSource != null)
            audioSource.PlayOneShot(skillSFX);
        gameObject.SetActive(true);
        LogDebug($"Launch. damage={runtimeDamage}, target={(targetEnemy != null ? targetEnemy.name : "null")}");
    }

    private void Update()
    {
        if (didHit)
        {
            return;
        }

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            LogDebug("Target missing. Release.");
            Release();
            return;
        }

        Vector3 direction = target.position - transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDir = direction.normalized;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
            transform.forward = moveDir;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other, null);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3? hitPoint = null;
        if (collision != null && collision.contactCount > 0)
        {
            hitPoint = collision.GetContact(0).point;
        }

        HandleHit(collision != null ? collision.collider : null, hitPoint);
    }

    private void HandleHit(Collider other, Vector3? hitPoint)
    {
        if (didHit || !IsTargetCollision(other))
        {
            return;
        }

        didHit = true;
        LogDebug($"Hit: {other.name}");

        EnemyHPController enemyHp = other.GetComponentInParent<EnemyHPController>() ?? other.GetComponent<EnemyHPController>();
        if (enemyHp != null)
        {
            enemyHp.TakeDamage(runtimeDamage);
            LogDebug($"Damage applied: {runtimeDamage}");
        }

        SpawnImpactVfx(other, hitPoint);
        if (impactSFX != null)
            MonImpactSoundManager.Instance?.PlaySound(impactSFX);
        Release();
    }

    private bool IsTargetCollision(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        Transform otherTransform = other.transform;

        if (target != null &&
            (otherTransform == target || otherTransform.IsChildOf(target) || target.IsChildOf(otherTransform)))
        {
            return true;
        }

        return ownerSkill != null && other.CompareTag(ownerSkill.EnemyTag);
    }

    private double ResolveDamage(PokemonData pokemonData, int level)
    {
        if (pokemonData != null && pokemonData.TryGetStatValueByLevel(level, "damage", out double damage))
        {
            return damage;
        }

        Debug.LogWarning("[SpriggleSkill] Missing 'damage' stat in PokemonData. Damage = 0.");
        return 0;
    }

    private void SpawnImpactVfx(Collider hitCollider, Vector3? overridePosition)
    {
        if (spriggleImpact == null)
        {
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
                SkillPoolToken token = impact.GetComponent<SkillPoolToken>();
                if (token != null)
                {
                    token.ReturnToPoolAfterDelay(impactLifeTime);
                }
                return;
            }
        }

        GameObject fallback = Instantiate(spriggleImpact, spawnPosition, Quaternion.identity);
        if (impactLifeTime > 0f)
        {
            Destroy(fallback, impactLifeTime);
        }
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
        target = null;
        audioSource?.Stop();
        ResetRigidbodyState();
    }

    private void ConfigurePhysicsForTriggerOnly()
    {
        if (cachedColliders == null || cachedColliders.Length == 0)
        {
            cachedColliders = GetComponentsInChildren<Collider>(true);
        }

        for (int i = 0; i < cachedColliders.Length; i++)
        {
            if (cachedColliders[i] != null)
            {
                cachedColliders[i].isTrigger = true;
            }
        }

        if (cachedRigidbody == null)
        {
            cachedRigidbody = GetComponent<Rigidbody>();
        }

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
        if (cachedRigidbody == null)
        {
            return;
        }

        cachedRigidbody.linearVelocity = Vector3.zero;
        cachedRigidbody.angularVelocity = Vector3.zero;
    }

    private void LogDebug(string message)
    {
        if (!enableDebugLog)
        {
            return;
        }

        Debug.Log($"[SpriggleSkill:{name}] {message}", this);
    }
}
