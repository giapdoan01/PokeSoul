using UnityEngine;

public class VeluneSkill : MonoBehaviour, IPokemonSkillLaunch
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog;

    [Header("Beam")]
    [SerializeField] private GameObject beamImpactVFX;
    [SerializeField] private int impactPreloadAmount = 3;
    [SerializeField] private int maxBeams = 3;
    [SerializeField] private float targetHeightOffset = 1f;

    [Header("Tick Damage")]
    [SerializeField] private float tickInterval = 0.1f;

    private const string ImpactPoolKey = "VeluneBeamImpact";

    [Header("SFX")]
    public AudioClip skillSFX;
    public AudioClip impactSFX;
    public AudioSource audioSource;

    private PokemonSkill _ownerSkill;
    private Transform _target;
    private EnemyMoveController _targetMove;
    private double _tickDamage;
    private float _tickTimer;
    private LineRenderer _lineRenderer;
    private GameObject _activeImpact;
    private bool _spawnedExtras;

    // ── Init ──

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer != null)
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.useWorldSpace = true;
            if (_lineRenderer.material == null)
                Debug.LogWarning("[VeluneSkill] LineRenderer has no material! Assign one in the prefab.");
        }

        RegisterImpactPool();
    }

    private void RegisterImpactPool()
    {
        if (beamImpactVFX == null || SkillObjectPolling.Instance == null) return;
        SkillObjectPolling.Instance.RegisterPrefab(ImpactPoolKey, beamImpactVFX, impactPreloadAmount);
    }

    // ── IPokemonSkillLaunch ──

    public void Launch(PokemonSkill owner, Transform targetEnemy, PokemonData pokemonData, int level, string attack)
    {
        LaunchInternal(owner, targetEnemy, pokemonData, level, attack, true);
    }

    public void LaunchInternal(PokemonSkill owner, Transform targetEnemy, PokemonData pokemonData, int level, string attack, bool spawnExtra)
    {
        _ownerSkill = owner;
        _target     = targetEnemy;
        _targetMove = targetEnemy?.GetComponent<EnemyMoveController>();
        _tickDamage = ResolveStat(pokemonData, level, "damage");
        _tickTimer  = 0f;

        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = true;
            _lineRenderer.positionCount = 2;
        }

        if (spawnExtra && !_spawnedExtras)
        {
            _spawnedExtras = true;
            SpawnExtraBeams(pokemonData, level, attack);
        }

        RefreshBeam();
        SpawnImpact();

        audioSource?.PlayOneShot(skillSFX);
        gameObject.SetActive(true);
        LogDebug($"Beam started. target={_target?.name}, tickDmg={_tickDamage}");
    }

    // ── Beam update ──

    private void Update()
    {
        if (_target == null || !_target.gameObject.activeInHierarchy) { Release(); return; }

        // Kiểm tra range — nếu target ra ngoài → dừng
        float dist = Vector3.Distance(_ownerSkill.CastPoint.position, GetTargetPos());
        if (dist > _ownerSkill.AttackRange) { Release(); return; }

        RefreshBeam();

        // Đuổi impact theo target
        if (_activeImpact != null)
            _activeImpact.transform.position = GetTargetPos();

        // Tick damage
        _tickTimer -= Time.deltaTime;
        if (_tickTimer <= 0f)
        {
            _tickTimer = tickInterval;
            ApplyTick();
        }
    }

    private void RefreshBeam()
    {
        if (_lineRenderer == null || _ownerSkill == null) return;
        _lineRenderer.SetPosition(0, _ownerSkill.CastPoint.position);
        _lineRenderer.SetPosition(1, GetTargetPos());
    }

    private Vector3 GetTargetPos()
    {
        Vector3 basePos = (_targetMove != null) ? _targetMove.FootPosition
            : (_target != null) ? _target.position : transform.position;
        return basePos + Vector3.up * targetHeightOffset;
    }

    // ── Damage ──

    private void ApplyTick()
    {
        if (_target == null || !_target.gameObject.activeInHierarchy) return;
        var hp = _target.GetComponentInParent<EnemyHPController>();
        if (hp != null) hp.TakeDamage(_tickDamage);
    }

    // ── Impact VFX ──

    private void SpawnImpact()
    {
        CleanupImpact();
        if (beamImpactVFX == null || _target == null) return;

        _activeImpact = SkillObjectPolling.Instance != null
            ? SkillObjectPolling.Instance.GetFromPool(ImpactPoolKey, GetTargetPos(), Quaternion.identity)
            : Instantiate(beamImpactVFX, GetTargetPos(), Quaternion.identity);

        if (_activeImpact == null) return;

        // Impact tồn tại đến khi beam dừng hoặc enemy chết — token không tự return
        // Sẽ được cleanup khi Release()
        var token = _activeImpact.GetComponent<SkillPoolToken>();
        if (token != null) token.CancelAutoReturn();
    }

    private void CleanupImpact()
    {
        if (_activeImpact == null) return;
        if (SkillObjectPolling.Instance != null)
            SkillObjectPolling.Instance.ReturnByInstance(_activeImpact);
        else
            Destroy(_activeImpact);
        _activeImpact = null;
    }

    // ── Multi-beam extras ──

    private void SpawnExtraBeams(PokemonData pokemonData, int level, string attack)
    {
        if (maxBeams <= 1) return;

        var targets = EnemyRegistry.Instance?.GetTargetsInRange(_ownerSkill.CastPoint.position, _ownerSkill.AttackRange, maxBeams);
        if (targets == null || targets.Count <= 1) return;

        for (int i = 1; i < targets.Count; i++)
        {
            if (targets[i] == null || targets[i] == _target) continue;

            var extraObj = _ownerSkill.GetSkillObjectFromPool(_ownerSkill.CastPoint.position, Quaternion.identity);
            if (extraObj == null) continue;

            var extraBeam = extraObj.GetComponent<VeluneSkill>();
            if (extraBeam == null) { _ownerSkill.ReleaseSkillObject(extraObj); continue; }

            extraBeam.LaunchInternal(_ownerSkill, targets[i], pokemonData, level, attack, false);
        }
    }

    // ── Release ──

    private void Release()
    {
        if (_lineRenderer != null) _lineRenderer.enabled = false;
        CleanupImpact();
        if (impactSFX != null) MonImpactSoundManager.Instance?.PlaySound(impactSFX);

        if (_ownerSkill != null) { _ownerSkill.ReleaseSkillObject(gameObject); return; }
        if (SkillObjectPolling.Instance != null) { SkillObjectPolling.Instance.ReturnByInstance(gameObject); return; }
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        CleanupImpact();
        _target     = null;
        _targetMove = null;
        _tickTimer  = 0f;
        _spawnedExtras = false;
        audioSource?.Stop();
    }

    // ── Stat ──

    private static double ResolveStat(PokemonData data, int level, string statName)
    {
        if (data != null && data.TryGetStatValueByLevel(level, statName, out double val)) return val;
        Debug.LogWarning($"[VeluneSkill] Missing stat '{statName}' at level {level}");
        return 0;
    }

    private void LogDebug(string message)
    {
        if (!enableDebugLog) return;
        Debug.Log($"[VeluneSkill:{name}] {message}", this);
    }
}
