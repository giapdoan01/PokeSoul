using UnityEngine;

public class PokemonSkill : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog;

    [Header("Data")]
    [SerializeField] private PokemonData pokemonData;
    [SerializeField] private int level = 1;

    [Header("Skill")]
    [SerializeField] private string skillComponent = "HomurinSkill";

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackAnimationTrigger = "attack";
    [SerializeField] private float minAnimatorSpeed = 0.5f;
    [SerializeField] private float maxAnimatorSpeed = 3f;

    [Header("Combat")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private Transform castPoint;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float defaultAttackCooldown = 1f;
    [SerializeField] private float minAttackCooldown = 0.05f;

    [Header("Pooling")]
    [SerializeField] private int preloadAmount = 5;

    private string poolKey;
    private float cooldownTimer;
    private float attackCooldown;
    private const float baseAnimatorSpeed = 1f;
    private float attackAnimatorSpeed = 1f;
    private float attackAnimationClipDuration = -1f;
    private Transform currentTarget;
    private bool isSkillRegistered;

    public string EnemyTag => enemyTag;
    public Transform CastPoint => castPoint != null ? castPoint : transform;
    public string SkillComponent => skillComponent;
    public int Level => level;
    public PokemonData PokemonData => pokemonData;
    public float AttackRange => attackRange;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (castPoint == null)
        {
            castPoint = transform;
        }

        attackAnimationClipDuration = ResolveAttackClipDuration();

        BuildPoolKey();
        ApplyStatsFromLevel();
        RegisterSkillPrefab();
        LogDebug($"Awake done. poolKey={poolKey}, cooldown={attackCooldown}, castPoint={CastPoint.name}");
    }

    private void OnValidate()
    {
        level = Mathf.Max(1, level);
        BuildPoolKey();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (castPoint == null)
        {
            castPoint = transform;
        }

        ApplyStatsFromLevel();
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        if (!TryFindNearestEnemy(out currentTarget))
        {
            return;
        }

        RotateToTarget(currentTarget);
        cooldownTimer = attackCooldown;
        LogDebug($"Trigger attack animation. target={currentTarget.name}, cooldown={attackCooldown}");

        if (animator != null && !string.IsNullOrWhiteSpace(attackAnimationTrigger))
        {
            animator.speed = attackAnimatorSpeed;
            animator.SetTrigger(attackAnimationTrigger);
            return;
        }

        CastSkillAnimationEvent(attackAnimationTrigger);
    }

    public void CastSkillAnimationEvent(string attack)
    {
        LogDebug($"CastSkillAnimationEvent called. attack={attack}");

        if (!CanCast())
        {
            LogDebug("CastSkillAnimationEvent stopped by CanCast().");
            return;
        }

        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            TryFindNearestEnemy(out currentTarget);
        }

        if (currentTarget == null)
        {
            LogDebug("CastSkillAnimationEvent stopped because currentTarget is null.");
            return;
        }

        Vector3 spawnPosition = CastPoint.position;
        Quaternion spawnRotation = GetSpawnRotation(spawnPosition, currentTarget);
        LogDebug($"Request pooled skill. position={spawnPosition}, target={currentTarget.name}");

        GameObject skillObject = GetSkillObjectFromPool(spawnPosition, spawnRotation);
        if (skillObject == null)
        {
            LogDebug("GetSkillObjectFromPool returned null.");
            return;
        }

        LogDebug($"Skill object spawned: {skillObject.name}");

        if (!LaunchSkillComponent(skillObject, currentTarget, attack))
        {
            LogDebug($"LaunchSkillComponent failed for {skillObject.name}");
            ReleaseSkillObject(skillObject);
        }
    }

    public void CastSkillAnimationEvent()
    {
        CastSkillAnimationEvent(attackAnimationTrigger);
    }

    public void OnAttackAnimationEnd()
    {
        if (animator != null)
        {
            animator.speed = baseAnimatorSpeed;
        }
    }

    public void SetLevel(int newLevel)
    {
        level = Mathf.Max(1, newLevel);
        ApplyStatsFromLevel();
        LogDebug($"SetLevel called. level={level}, cooldown={attackCooldown}");
    }

    public void ReleaseSkillObject(GameObject skillObject)
    {
        if (skillObject == null)
        {
            return;
        }

        LogDebug($"Release skill object: {skillObject.name}");

        if (SkillObjectPolling.Instance != null)
        {
            SkillObjectPolling.Instance.ReturnByInstance(skillObject);
            return;
        }

        Destroy(skillObject);
    }

    public GameObject GetSkillObjectFromPool(Vector3 position, Quaternion rotation)
    {
        if (!EnsureSkillRegistered())
        {
            return null;
        }

        GameObject skillObject = SkillObjectPolling.Instance.GetFromPool(poolKey, position, rotation);
        if (skillObject != null)
        {
            LogDebug($"GetSkillObjectFromPool success: {skillObject.name}");
        }
        return skillObject;
    }

    private bool LaunchSkillComponent(GameObject skillObject, Transform target, string attack)
    {
        Component skill = skillObject.GetComponent(skillComponent);
        if (skill == null)
        {
            Debug.LogWarning($"[PokemonSkill] Cannot find component '{skillComponent}' on '{skillObject.name}'.");
            return false;
        }

        if (skill is IPokemonSkillLaunch launchableSkill)
        {
            LogDebug($"Launch component found: {skill.GetType().Name}");
            launchableSkill.Launch(this, target, pokemonData, level, attack);
            return true;
        }

        Debug.LogWarning($"[PokemonSkill] Component '{skillComponent}' must implement IPokemonSkillLaunch.");
        return false;
    }

    private void ApplyStatsFromLevel()
    {
        attackCooldown = GetAttackCooldownFromStat(level);
        UpdateAnimationSpeedByCooldown();
    }

    private float GetAttackCooldownFromStat(int statLevel)
    {
        if (pokemonData != null && pokemonData.TryGetStatValueByLevel(statLevel, "cd", out double cdStat))
        {
            return Mathf.Max(minAttackCooldown, (float)cdStat);
        }

        return Mathf.Max(minAttackCooldown, defaultAttackCooldown);
    }

    private Quaternion GetSpawnRotation(Vector3 spawnPosition, Transform target)
    {
        if (target == null)
        {
            return CastPoint.rotation;
        }

        Vector3 direction = target.position - spawnPosition;
        if (direction.sqrMagnitude <= 0.001f)
        {
            return CastPoint.rotation;
        }

        return Quaternion.LookRotation(direction.normalized);
    }

    private void UpdateAnimationSpeedByCooldown()
    {
        if (animator == null)
        {
            return;
        }

        if (attackAnimationClipDuration < 0f)
        {
            attackAnimationClipDuration = ResolveAttackClipDuration();
        }

        if (attackAnimationClipDuration <= 0f)
        {
            attackAnimatorSpeed = baseAnimatorSpeed;
            return;
        }

        float targetSpeed = attackAnimationClipDuration / Mathf.Max(minAttackCooldown, attackCooldown);
        attackAnimatorSpeed = Mathf.Clamp(targetSpeed, minAnimatorSpeed, maxAnimatorSpeed);
    }

    private float ResolveAttackClipDuration()
    {
        if (animator == null || string.IsNullOrWhiteSpace(attackAnimationTrigger))
        {
            return -1f;
        }

        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (controller == null)
        {
            return -1f;
        }

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name.ToLower().Contains(attackAnimationTrigger.ToLower()))
            {
                LogDebug($"Attack clip found: {clip.name}, duration={clip.length}");
                return clip.length;
            }
        }

        LogDebug($"No attack clip found matching '{attackAnimationTrigger}'. Speed not scaled.");
        return -1f;
    }

    private bool TryFindNearestEnemy(out Transform nearestTarget)
    {
        if (EnemyRegistry.Instance != null)
        {
            bool found = EnemyRegistry.Instance.TryGetPriorityTarget(transform.position, attackRange, out nearestTarget);
            if (found) LogDebug($"Target found via EnemyRegistry: {nearestTarget.name}");
            return found;
        }

        // Fallback nếu EnemyRegistry chưa có trong scene
        nearestTarget = null;
        return false;
    }

    private void RotateToTarget(Transform target)
    {
        if (target == null)
        {
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    private bool CanCast()
    {
        if (pokemonData == null)
        {
            Debug.LogWarning($"[PokemonSkill] PokemonData is null on '{name}'.");
            return false;
        }

        if (pokemonData.skillPrefab == null)
        {
            Debug.LogWarning($"[PokemonSkill] skillPrefab is null on PokemonData '{pokemonData.name}'.");
            return false;
        }

        if (SkillObjectPolling.Instance == null)
        {
            Debug.LogWarning("[PokemonSkill] Need SkillObjectPolling in scene.");
            return false;
        }

        return true;
    }

    private bool EnsureSkillRegistered()
    {
        if (!CanCast())
        {
            return false;
        }

        if (!isSkillRegistered)
        {
            RegisterSkillPrefab();
        }

        return isSkillRegistered;
    }

    private void RegisterSkillPrefab()
    {
        if (!CanCast())
        {
            return;
        }

        SkillObjectPolling.Instance.RegisterPrefab(poolKey, pokemonData.skillPrefab, preloadAmount);
        isSkillRegistered = true;
        LogDebug($"Register skill prefab done. prefab={pokemonData.skillPrefab.name}, preload={preloadAmount}, poolKey={poolKey}");
    }

    private void BuildPoolKey()
    {
        string pokemonId = pokemonData != null && !string.IsNullOrWhiteSpace(pokemonData.id)
            ? pokemonData.id
            : name;

        poolKey = pokemonId + "_" + skillComponent;
        isSkillRegistered = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void LogDebug(string message)
    {
        if (!enableDebugLog)
        {
            return;
        }

        Debug.Log($"[PokemonSkill:{name}] {message}", this);
    }
}
