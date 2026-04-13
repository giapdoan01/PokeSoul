using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyMoveController : MonoBehaviour
{
    public EnemyData enemyData;
    
    public WayPointForEnemy wayPointManager;
    
    private int currentWayPointIndex = 0;
    private Transform currentWayPoint;
    private double moveSpeed;
    private bool reachedEnd = false;
    private EnemyWaveData enemyWaveData;

    public void SetSpeedByWave (int waveIndex)
    {
        enemyWaveData = enemyData.getEnemyWaveDataByName(waveIndex);
        if (enemyWaveData != null)
        {
            moveSpeed = enemyWaveData.enemyStats.speed;
        }
    }
    
    void Start()
    {
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

        SetSpeedByWave(1);
    }
    
    void Update()
    {
        if (reachedEnd || currentWayPoint == null)
            return;
            
        // Di chuyển enemy đến waypoint hiện tại
        Vector3 direction = currentWayPoint.position - transform.position;
        float distanceThisFrame = (float)moveSpeed * Time.deltaTime;
        
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
    
    // Phương thức được gọi khi đến điểm cuối cùng
    void OnReachEndPoint()
    {
       
    }
}