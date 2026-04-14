using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using UnityEngine.UI;


public class WaveManager : MonoBehaviour
{
    public Map mapData;
    public Button nextWaveButton;
    private int currentWaveNumber;
    private EnemyData[] enemyBattleDatas;
    private WayPointForEnemy wayPointSystem;

    void Awake()
    {
        if (mapData == null)
        {
            Debug.LogError("[WaveManager] Không tìm thấy Map Data! Vui lòng gán Map Data trong Inspector.");
            return;
        }
        currentWaveNumber = 1;
        enemyBattleDatas = mapData.enemyDatas;
        
        // Tìm WayPointForEnemy trong scene
        wayPointSystem = FindObjectOfType<WayPointForEnemy>();
        if (wayPointSystem == null)
        {
            Debug.LogError("[WaveManager] Không tìm thấy WayPointForEnemy trong scene!");
        }
        else if (wayPointSystem.wayPoints.Count == 0)
        {
            Debug.LogError("[WaveManager] WayPointForEnemy không có waypoint nào!");
        }

        if (nextWaveButton != null)
        {
            nextWaveButton.onClick.AddListener(StartNextWave);
        }
        else
        {
            Debug.LogWarning("[WaveManager] Không tìm thấy Next Wave Button! Vui lòng gán Button trong Inspector.");
        }
    }
    
    void Start()
    {
        StartNextWave();
    }

    public void StartNextWave()
    {
        Debug.Log($"[WaveManager] Bắt đầu wave {currentWaveNumber}");
        StartCoroutine(SpawnEnemiesForCurrentWave());
        currentWaveNumber++;
    }
    
    private IEnumerator SpawnEnemiesForCurrentWave()
    {
        // Kiểm tra WayPointForEnemy
        if (wayPointSystem == null || wayPointSystem.wayPoints.Count == 0)
        {
            Debug.LogError("[WaveManager] Không thể spawn enemy vì không tìm thấy waypoint!");
            yield break;
        }

        // Lấy vị trí spawn (waypoint đầu tiên)
        Transform spawnPoint = wayPointSystem.wayPoints[0];
        
        foreach (var enemyData in enemyBattleDatas)
        {
            EnemyWaveData waveData = enemyData.getEnemyWaveDataByName(currentWaveNumber);
            if (waveData != null)
            {
                for (int i = 0; i < waveData.enemyStats.count; i++)
                {
                    // Spawn enemy tại vị trí waypoint đầu tiên
                    GameObject enemyInstance = Instantiate(enemyData.enemyPrefab, spawnPoint.position, Quaternion.identity);
                    enemyInstance.GetComponent<EnemyHPController>().setHpByWaveName(currentWaveNumber);
                    
                    // Đợi 1 giây trước khi sinh ra enemy tiếp theo
                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                Debug.LogWarning($"[WaveManager] Không tìm thấy dữ liệu wave {currentWaveNumber} cho enemy {enemyData.enemyName}");
            }
        }
    }

    public void SetMapData(Map newMapData)
    {
        mapData = newMapData;
    }
}