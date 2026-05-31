using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyMoveController : MonoBehaviour
{
    private EnemyStatusEffects _status;
    private EnemyData enemyData;

    public WayPointForEnemy wayPointManager;

    [SerializeField] private Transform footPoint;

    public Vector3 FootPosition => footPoint != null ? footPoint.position : transform.position;
    
    public int CurrentWayPointIndex => currentWayPointIndex;

    private int currentWayPointIndex = 0;
    private Transform currentWayPoint;
    private double moveSpeed;
    private bool reachedEnd = false;
    private EnemyWaveData enemyWaveData;

    public void SetEnemyData(EnemyData data)
    {
        enemyData = data;
    }

    public void SetSpeedByWave(int waveIndex)
    {
        enemyWaveData = enemyData.getEnemyWaveDataByName(waveIndex);
        if (enemyWaveData != null)
            moveSpeed = enemyWaveData.enemyStats.speed;
    }

    public void ResetForReuse(WayPointForEnemy wayPoint)
    {
        wayPointManager = wayPoint;
        currentWayPointIndex = 0;
        reachedEnd = false;
        currentWayPoint = null;
        _status = GetComponent<EnemyStatusEffects>();

        if (wayPointManager != null && wayPointManager.wayPoints.Count > 0)
            wayPointManager.getWayPoint(0, out currentWayPoint);
    }
    
    void Start()
    {
        _status = GetComponent<EnemyStatusEffects>();

        if (wayPointManager == null)
        {
            wayPointManager = FindObjectOfType<WayPointForEnemy>();
            if (wayPointManager == null)
            {
                Debug.LogError("[EnemyMoveController] Không tìm thấy WayPointForEnemy trong scene!");
                return;
            }
        }
        
        if (wayPointManager.wayPoints.Count == 0)
        {
            Debug.LogWarning("[EnemyMoveController] Danh sách waypoint trống!");
            return;
        }
        
        wayPointManager.getWayPoint(currentWayPointIndex, out currentWayPoint);
    }
    
    void Update()
    {
        if (reachedEnd || currentWayPoint == null)
            return;

        if (_status != null && _status.IsImmobilized)
            return;

        Vector3 direction = currentWayPoint.position - transform.position;
        float speed = (float)moveSpeed * (_status != null ? _status.SpeedMultiplier : 1f);
        float distanceThisFrame = speed * Time.deltaTime;
        
        // Quay mặt enemy theo hướng di chuyển
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }
        
        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        
        if (Vector3.Distance(transform.position, currentWayPoint.position) < 0.2f)
        {
            currentWayPointIndex++;
            
            if (currentWayPointIndex >= wayPointManager.wayPoints.Count)
            {
                reachedEnd = true;
                OnReachEndPoint();
            }
            else
            {
                wayPointManager.getWayPoint(currentWayPointIndex, out currentWayPoint);
            }
        }
    }
    
    void OnReachEndPoint()
    {
        MatchTracker.Instance?.RegisterEnemyReachedEnd();

        if (EnemyObjectPool.Instance != null)
            EnemyObjectPool.Instance.Return(gameObject);
        else
            gameObject.SetActive(false);
    }
}