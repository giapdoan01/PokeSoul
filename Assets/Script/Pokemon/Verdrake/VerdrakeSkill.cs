using UnityEngine;
using System.Collections.Generic;

public class VerdrakeSkill : MonoBehaviour, IPokemonSkillLaunch
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog;

    [SerializeField] private GameObject verdrakeImpactPrefab;
    [SerializeField] private float impactLifeTime = 1.5f;
    [SerializeField] private int impactPreloadAmount = 5;

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

    private VerdrakeParticleController particleController;

    private void Awake()
    {
        RegisterPools();
        particleController = GetComponent<VerdrakeParticleController>();
    }

    private void RegisterPools()
    {
        if (SkillObjectPolling.Instance == null) return;
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

        Vector3 footPos = targetMoveController != null
            ? targetMoveController.FootPosition
            : targetEnemy.position;

        transform.position = new Vector3(footPos.x, 0.14f, footPos.z);

        if (particleController != null)
            particleController.Initialize(verdrakeImpactPrefab, impactLifeTime);

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
        if (didFinish || targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy) return;

        Vector3 footPos = targetMoveController != null
            ? targetMoveController.FootPosition
            : targetEnemy.position;

        transform.position = new Vector3(footPos.x, 0.14f, footPos.z);
    }

    public void OnParticleArrived()
    {
        if (didFinish) return;
        LogDebug("Particle arrived. Finish.");

        if (!didDamage)
            ApplyDamageToTarget();

        if (impactSFX != null)
            MonImpactSoundManager.Instance?.PlaySound(impactSFX);
        Release();
    }

    private void ApplyDamageToTarget()
    {
        if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy) return;

        EnemyHPController hp = targetEnemy.GetComponentInChildren<EnemyHPController>()
            ?? targetEnemy.GetComponent<EnemyHPController>();

        if (hp != null)
        {
            hp.TakeDamage(runtimeDamage);
            didDamage = true;
            LogDebug($"Damage applied via OnParticleArrived: {runtimeDamage}");
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

    private void SpawnExtraSkills(PokemonData pokemonData, int level, string attack)
    {
        if (ownerSkill == null) return;

        int targetCount = ResolveTargetCount(pokemonData, level);
        if (targetCount <= 1) return;

        Transform[] targets = FindPriorityTargets(targetCount);

        for (int i = 1; i < targets.Length; i++)
        {
            if (targets[i] == null || targets[i] == targetEnemy) continue;

            Vector3 spawnPos = targets[i].position;
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
        targetEnemy = null;
        targetMoveController = null;
        audioSource?.Stop();
    }

    private void LogDebug(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[VerdrakeSkill:{name}] {message}", this);
    }
}
