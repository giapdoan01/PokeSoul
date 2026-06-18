using UnityEngine;
using Cysharp.Threading.Tasks;

public class GekigumaSkill : MonoBehaviour, IPokemonSkillLaunch
{
    [Header("Paw Config")]
    [SerializeField] private GameObject pawPrefab;
    [SerializeField] private float preCastDelay = 0.5f;
    [SerializeField] private float pawSpeed = 8f;
    [SerializeField] private int pawPreload = 1;

    private const string PawPoolKey = "GekigumaPaw";

    [Header("SFX")]
    public AudioClip skillSFX;
    public AudioClip impactSFX;
    public AudioSource audioSource;

    private PokemonSkill _ownerSkill;

    private void Awake()
    {
        if (pawPrefab != null && SkillObjectPolling.Instance != null)
            SkillObjectPolling.Instance.RegisterPrefab(PawPoolKey, pawPrefab, pawPreload);
    }

    public void Launch(PokemonSkill owner, Transform targetEnemy, PokemonData pokemonData, int level, string attack)
    {
        _ownerSkill = owner;

        // Tìm random enemy trong tầm
        var targets = EnemyRegistry.Instance?.GetTargetsInRange(owner.CastPoint.position, owner.AttackRange, 1);
        Transform picked = (targets != null && targets.Count > 0) ? targets[0] : null;

        Vector3 pawAPos, pawBPos;

        if (picked != null)
        {
            pawAPos = picked.position + Vector3.up * 0.1f;

            // Vị trí waypoint tiếp theo của enemy làm pawB
            var move = picked.GetComponent<EnemyMoveController>();
            if (move != null && move.wayPointManager != null && move.CurrentWayPointIndex < move.wayPointManager.wayPoints.Count)
            {
                pawBPos = move.wayPointManager.wayPoints[move.CurrentWayPointIndex].position + Vector3.up * 0.1f;
            }
            else
            {
                // Fallback: đặt pawB xa hơn về phía trước
                pawBPos = pawAPos + owner.CastPoint.forward * 4f;
            }
        }
        else
        {
            // Fallback khi không có enemy trong tầm
            pawAPos = owner.CastPoint.position + owner.CastPoint.forward * 2f + Vector3.up * 0.1f;
            pawBPos = owner.CastPoint.position + owner.CastPoint.forward * 5f + Vector3.up * 0.1f;
        }

        double damage = ResolveStat(pokemonData, level, "damage");

        var pawA = SpawnPaw(pawAPos, picked, damage, true);
        var pawB = SpawnPaw(pawBPos, picked, damage, false);

        if (pawA != null) pawA.Launch(pawB, damage, preCastDelay, pawSpeed, impactSFX, skillSFX, true);
        if (pawB != null) pawB.Launch(pawA, damage, preCastDelay, pawSpeed, impactSFX, null, false);

        // Main object không cần giữ lại
        gameObject.SetActive(false);
    }

    private GekigumaPaw SpawnPaw(Vector3 spawnPos, Transform enemy, double damage, bool playLoop)
    {
        GameObject go = SkillObjectPolling.Instance != null
            ? SkillObjectPolling.Instance.GetFromPool(PawPoolKey, spawnPos, Quaternion.identity)
            : Instantiate(pawPrefab, spawnPos, Quaternion.identity);

        if (go == null) return null;
        return go.GetComponent<GekigumaPaw>();
    }

    private void OnDisable()
    {
        audioSource?.Stop();
    }

    private static double ResolveStat(PokemonData data, int level, string statName)
    {
        if (data != null && data.TryGetStatValueByLevel(level, statName, out double val)) return val;
        return 0;
    }
}
