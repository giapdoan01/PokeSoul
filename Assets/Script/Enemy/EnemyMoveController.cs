using UnityEngine;

public class EnemyMoveController : MonoBehaviour
{
    public WayPointForEnemy wayPointManager;

    [SerializeField] private Transform footPoint;

    public Vector3 FootPosition  => footPoint != null ? footPoint.position : transform.position;
    public int CurrentWayPointIndex => _waypointIndex;

    private EnemyData _enemyData;
    private EnemyStatusEffects _status;
    private Transform _currentWaypoint;
    private double _moveSpeed;
    private int _waypointIndex;
    private bool _reachedEnd;

    // ── Setup ──

    public void SetEnemyData(EnemyData data) => _enemyData = data;

    public void SetSpeedByWave(int waveIndex)
    {
        var waveData = _enemyData?.getEnemyWaveDataByName(waveIndex);
        if (waveData != null)
            _moveSpeed = waveData.enemyStats.speed;
    }

    public void ResetForReuse(WayPointForEnemy wayPoint)
    {
        wayPointManager  = wayPoint;
        _waypointIndex   = 0;
        _reachedEnd      = false;
        _currentWaypoint = null;
        _status          = GetComponent<EnemyStatusEffects>();

        if (wayPointManager != null && wayPointManager.wayPoints.Count > 0)
            wayPointManager.getWayPoint(0, out _currentWaypoint);
    }

    // ── Lifecycle ──

    private void OnEnable()  => EnemyRegistry.Instance?.Register(this);
    private void OnDisable() => EnemyRegistry.Instance?.Unregister(this);

    private void Start()
    {
        _status = GetComponent<EnemyStatusEffects>();

        // Fallback nếu chưa được inject qua ResetForReuse
        if (wayPointManager == null)
            wayPointManager = FindObjectOfType<WayPointForEnemy>();

        if (wayPointManager == null || wayPointManager.wayPoints.Count == 0)
        {
            Debug.LogError("[EnemyMoveController] Không tìm thấy WayPointForEnemy hoặc waypoints trống!");
            return;
        }

        wayPointManager.getWayPoint(_waypointIndex, out _currentWaypoint);
    }

    // ── Movement ──

    private void Update()
    {
        if (_reachedEnd || _currentWaypoint == null) return;
        if (_status != null && _status.IsImmobilized) return;

        float speed = (float)_moveSpeed * (_status != null ? _status.SpeedMultiplier : 1f);
        Vector3 direction = _currentWaypoint.position - transform.position;

        // Quay mặt theo hướng di chuyển
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);

        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

        // Dùng sqrMagnitude thay Vector3.Distance — tránh sqrt mỗi frame
        if (direction.sqrMagnitude < 0.04f) // 0.04 = 0.2 * 0.2
            AdvanceWaypoint();
    }

    private void AdvanceWaypoint()
    {
        _waypointIndex++;

        if (_waypointIndex >= wayPointManager.wayPoints.Count)
        {
            _reachedEnd = true;
            OnReachEndPoint();
        }
        else
        {
            wayPointManager.getWayPoint(_waypointIndex, out _currentWaypoint);
        }
    }

    private void OnReachEndPoint()
    {
        MatchTracker.Instance?.RegisterEnemyReachedEnd();

        if (EnemyObjectPool.Instance != null)
            EnemyObjectPool.Instance.Return(gameObject);
        else
            gameObject.SetActive(false);
    }
}
