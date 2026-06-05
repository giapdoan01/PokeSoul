using System.Collections;
using UnityEngine;

public class ClodooSkill : MonoBehaviour, IPokemonSkillLaunch
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog;

    [Header("Projectile")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float delayBeforeFly = 0.5f;

    [Header("Impact")]
    [SerializeField] private GameObject clodooImpact;
    [SerializeField] private float impactLifeTime = 1.5f;
    [SerializeField] private float impactHeightOffset = 1f;
    [SerializeField] private int impactPreloadAmount = 5;

    [Header("SFX")]
    public AudioClip skillSFX;
    public AudioClip impactSFX;
    public AudioSource audioSource;

    private const string ImpactPoolKey = "ClodooImpact";

    private PokemonSkill ownerSkill;
    private Transform target;
    private double runtimeDamage;
    private bool didHit;
    private bool isFlying;
    private Collider[] cachedColliders;
    private Rigidbody cachedRigidbody;

    private void Awake()
    {
        cachedColliders = GetComponentsInChildren<Collider>(true);
        cachedRigidbody = GetComponent<Rigidbody>();
        ConfigurePhysics();
        RegisterImpactPool();
    }

    private void RegisterImpactPool()
    {
        if (clodooImpact == null || SkillObjectPolling.Instance == null) return;
        SkillObjectPolling.Instance.RegisterPrefab(ImpactPoolKey, clodooImpact, impactPreloadAmount);
    }

    public void Launch(PokemonSkill owner, Transform targetEnemy, PokemonData pokemonData, int level, string attack)
    {
        ownerSkill = owner;
        target = targetEnemy;
        runtimeDamage = ResolveDamage(pokemonData, level);
        didHit = false;
        isFlying = false;
        ConfigurePhysics();

        if (skillSFX != null && audioSource != null)
            audioSource.PlayOneShot(skillSFX);

        gameObject.SetActive(true);
        StartCoroutine(DelayThenFly());
        LogDebug($"Launch. target={target?.name}, damage={runtimeDamage}");
    }

    private IEnumerator DelayThenFly()
    {
        yield return new WaitForSeconds(delayBeforeFly);
        isFlying = true;
        LogDebug("Delay done, now flying toward target.");
    }

    private void Update()
    {
        if (didHit || !isFlying) return;

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
        Vector3? hitPoint = collision?.contactCount > 0 ? collision.GetContact(0).point : (Vector3?)null;
        HandleHit(collision?.collider, hitPoint);
    }

    private void HandleHit(Collider other, Vector3? hitPoint)
    {
        if (didHit || !isFlying) return;
        if (!IsTargetCollision(other)) return;

        didHit = true;
        LogDebug($"Hit: {other?.name}");

        EnemyHPController enemyHp = other.GetComponentInParent<EnemyHPController>()
                                 ?? other.GetComponent<EnemyHPController>();
        if (enemyHp != null)
        {
            enemyHp.TakeDamage(runtimeDamage);
            LogDebug($"Damage applied: {runtimeDamage}");
        }

        SpawnImpact(other, hitPoint);

        if (impactSFX != null)
            MonImpactSoundManager.Instance?.PlaySound(impactSFX);

        Release();
    }

    private bool IsTargetCollision(Collider other)
    {
        if (other == null) return false;
        if (target != null)
        {
            Transform t = other.transform;
            if (t == target || t.IsChildOf(target) || target.IsChildOf(t)) return true;
        }
        if (ownerSkill != null && other.CompareTag(ownerSkill.EnemyTag)) return true;
        return false;
    }

    private void SpawnImpact(Collider hitCollider, Vector3? overridePosition)
    {
        if (clodooImpact == null) return;

        Vector3 pos = overridePosition
            ?? (hitCollider != null ? hitCollider.ClosestPoint(transform.position) : transform.position);
        pos += Vector3.up * impactHeightOffset;

        if (SkillObjectPolling.Instance != null)
        {
            RegisterImpactPool();
            GameObject impact = SkillObjectPolling.Instance.GetFromPool(ImpactPoolKey, pos, Quaternion.identity);
            if (impact != null)
            {
                SkillPoolToken token = impact.GetComponent<SkillPoolToken>();
                if (token != null) token.ReturnToPoolAfterDelay(impactLifeTime);
                return;
            }
        }

        GameObject fallback = Instantiate(clodooImpact, pos, Quaternion.identity);
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
        StopAllCoroutines();
        didHit = false;
        isFlying = false;
        target = null;
        audioSource?.Stop();
        if (cachedRigidbody != null)
        {
            cachedRigidbody.linearVelocity = Vector3.zero;
            cachedRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private double ResolveDamage(PokemonData pokemonData, int level)
    {
        if (pokemonData != null && pokemonData.TryGetStatValueByLevel(level, "damage", out double dmg))
            return dmg;
        Debug.LogWarning("[ClodooSkill] Missing 'damage' stat. Damage = 0.");
        return 0;
    }

    private void ConfigurePhysics()
    {
        if (cachedColliders == null || cachedColliders.Length == 0)
            cachedColliders = GetComponentsInChildren<Collider>(true);

        foreach (var col in cachedColliders)
            if (col != null) col.isTrigger = true;

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

    private void LogDebug(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[ClodooSkill:{name}] {message}", this);
    }
}
