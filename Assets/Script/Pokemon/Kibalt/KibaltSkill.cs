using System.Collections.Generic;
using UnityEngine;

public class KibaltSkill : MonoBehaviour, IPokemonSkillLaunch
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog;

    [Header("Tick Damage")]
    [SerializeField] private float tickInterval = 0.2f;

    [Header("Spawn")]
    [SerializeField] private float spawnHeightOffset = 0f;

    [Header("SFX")]
    public AudioClip skillSFX;
    public AudioSource audioSource;

    private PokemonSkill ownerSkill;
    private double runtimeDamage;
    private float runtimeDuration;
    private float tickTimer;
    private float lifeTimer;
    private string enemyTag;
    private readonly List<EnemyHPController> enemiesInTrigger = new List<EnemyHPController>();

    public void Launch(PokemonSkill owner, Transform targetEnemy, PokemonData pokemonData, int level, string attack)
    {
        ownerSkill = owner;
        enemyTag = owner != null ? owner.EnemyTag : "Enemy";
        runtimeDamage = ResolveDamage(pokemonData, level);
        runtimeDuration = ResolveDuration(pokemonData, level);
        tickTimer = 0f;
        lifeTimer = Mathf.Max(0.01f, runtimeDuration);
        CleanupMissingEnemies();
        transform.position = ResolveSkillSpawnPosition(targetEnemy);
        if (skillSFX != null && audioSource != null)
        {
            audioSource.clip = skillSFX;
            audioSource.loop = true;
            audioSource.Play();
        }

        gameObject.SetActive(true);

        LogDebug($"Launch. damage={runtimeDamage}, duration={lifeTimer}, tick={tickInterval}");
        tickTimer = Mathf.Max(0.01f, tickInterval);
    }

    private void Update()
    {
        if (ownerSkill == null)
        {
            Release();
            return;
        }

        lifeTimer -= Time.deltaTime;
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            ApplyTickDamageToTrackedEnemies();
            tickTimer = Mathf.Max(0.01f, tickInterval);
        }

        if (lifeTimer <= 0f)
        {
            Release();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryTrackEnemy(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryTrackEnemy(other);
    }

    private void TryTrackEnemy(Collider other)
    {
        EnemyHPController enemyHp = GetEnemyHpFromCollider(other);
        if (enemyHp == null || enemiesInTrigger.Contains(enemyHp))
        {
            return;
        }

        enemiesInTrigger.Add(enemyHp);
        LogDebug($"Enemy entered AOE: {enemyHp.name}");
    }

    private void OnTriggerExit(Collider other)
    {
        EnemyHPController enemyHp = GetEnemyHpFromCollider(other);
        if (enemyHp == null)
        {
            return;
        }

        if (enemiesInTrigger.Remove(enemyHp))
        {
            LogDebug($"Enemy exited AOE: {enemyHp.name}");
        }
    }

    private void ApplyTickDamageToTrackedEnemies()
    {
        CleanupMissingEnemies();

        for (int i = 0; i < enemiesInTrigger.Count; i++)
        {
            EnemyHPController enemyHp = enemiesInTrigger[i];
            if (enemyHp == null || !enemyHp.gameObject.activeInHierarchy)
            {
                continue;
            }

            enemyHp.TakeDamage(runtimeDamage);
            LogDebug($"Tick damage applied to {enemyHp.name}: {runtimeDamage}");
        }
    }

    private EnemyHPController GetEnemyHpFromCollider(Collider other)
    {
        if (other == null || !IsEnemyCollider(other.transform))
        {
            return null;
        }

        return other.GetComponentInParent<EnemyHPController>() ?? other.GetComponent<EnemyHPController>();
    }

    private bool IsEnemyCollider(Transform target)
    {
        while (target != null)
        {
            if (!string.IsNullOrWhiteSpace(enemyTag) && target.CompareTag(enemyTag))
            {
                return true;
            }

            target = target.parent;
        }

        return false;
    }

    private double ResolveDamage(PokemonData pokemonData, int level)
    {
        if (pokemonData != null && pokemonData.TryGetStatValueByLevel(level, "damage", out double damage))
        {
            return damage;
        }

        Debug.LogWarning("[KibaltSkill] Missing 'damage' stat in PokemonData. Damage = 0.");
        return 0;
    }

    private float ResolveDuration(PokemonData pokemonData, int level)
    {
        if (pokemonData != null && pokemonData.TryGetStatValueByLevel(level, "duration", out double duration))
        {
            return Mathf.Max(0.01f, (float)duration);
        }

        Debug.LogWarning("[KibaltSkill] Missing 'duration' stat in PokemonData. Duration = 1.");
        return 1f;
    }

    private Vector3 ResolveSkillSpawnPosition(Transform targetEnemy)
    {
        Vector3 base_pos;

        if (targetEnemy != null)
            base_pos = targetEnemy.position;
        else if (ownerSkill != null)
            base_pos = ownerSkill.CastPoint.position;
        else
            base_pos = transform.position;

        return base_pos + Vector3.up * spawnHeightOffset;
    }

    private void CleanupMissingEnemies()
    {
        for (int i = enemiesInTrigger.Count - 1; i >= 0; i--)
        {
            EnemyHPController enemyHp = enemiesInTrigger[i];
            if (enemyHp == null || !enemyHp.gameObject.activeInHierarchy)
            {
                enemiesInTrigger.RemoveAt(i);
            }
        }
    }

    private void Release()
    {
        enemiesInTrigger.Clear();

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
        tickTimer = 0f;
        lifeTimer = 0f;
        runtimeDuration = 0f;
        ownerSkill = null;
        enemyTag = null;
        enemiesInTrigger.Clear();
        audioSource?.Stop();
    }

    private void LogDebug(string message)
    {
        if (!enableDebugLog)
        {
            return;
        }

        Debug.Log($"[KibaltSkill:{name}] {message}", this);
    }
}
