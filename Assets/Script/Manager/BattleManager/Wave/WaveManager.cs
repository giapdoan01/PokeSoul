using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using UnityEngine.UI;


public class WaveManager : MonoBehaviour
{
    public Map mapData;
    public PlayerStatsManager playerStatsManager;
    public int timeToStartFirstWave = 10;
    private int currentWaveNumber;
    private EnemyData[] enemyBattleDatas;
    private WayPointForEnemy wayPointSystem;
    public Action<int> OnCountdownTick;
    public Action OnCountdownFinished;

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
    }
    
    void Start()
    {
        StartNextWave();
    }

    public void StartNextWave()
    {
        Debug.Log($"[WaveManager] Bắt đầu wave {currentWaveNumber}");
        StartCoroutine(SpawnEnemiesForCurrentWave(currentWaveNumber));
        currentWaveNumber++;
    }

    private IEnumerator SpawnEnemiesForCurrentWave(int waveNumber)
    {
        if (waveNumber == 1)
        {
            Debug.Log($"[WaveManager] Bắt đầu đếm ngược {timeToStartFirstWave} giây trước khi bắt đầu wave 1");
            for (int t = timeToStartFirstWave; t > 0; t--)
            {
                OnCountdownTick?.Invoke(t);
                yield return new WaitForSeconds(1f);
            }
            OnCountdownFinished?.Invoke();
        }
        // Kiểm tra WayPointForEnemy
        if (wayPointSystem == null || wayPointSystem.wayPoints.Count == 0)
        {
            Debug.LogError("[WaveManager] Không thể spawn enemy vì không tìm thấy waypoint!");
            yield break;
        }
        //Cộng tiền wave này vào playerCoin
        int rewardCoin = 0;
        int bonusCoin = 0;
        WaveReward waveReward = mapData.getWaveRewardByWaveNumber(waveNumber);
        if (waveReward != null)
        {
            rewardCoin = waveReward.waveReward;
            bonusCoin = waveReward.waveSpecialReward;
            int totalReward = rewardCoin + bonusCoin;
            playerStatsManager.AddCoin(totalReward);
            Debug.Log($"[WaveManager] Kết thúc wave {waveNumber}. Thưởng {totalReward} coin.");
        }
        else
        {
            Debug.LogWarning($"[WaveManager] Không tìm thấy phần thưởng cho wave {waveNumber}");
        }

        // Lấy vị trí spawn (waypoint đầu tiên)
        Transform spawnPoint = wayPointSystem.wayPoints[0];

        foreach (var enemyData in enemyBattleDatas)
        {
            EnemyWaveData waveData = enemyData.getEnemyWaveDataByName(waveNumber);
            if (waveData != null)
            {
                for (int i = 0; i < waveData.enemyStats.count; i++)
                {
                    // Spawn enemy tại vị trí waypoint đầu tiên
                    GameObject enemyInstance = Instantiate(enemyData.enemyPrefab, spawnPoint.position, Quaternion.identity);

                    EnemyHPController hpController = enemyInstance.GetComponent<EnemyHPController>();
                    hpController.SetEnemyData(enemyData);
                    hpController.setHpByWaveName(waveNumber);

                    EnemyMoveController moveController = enemyInstance.GetComponent<EnemyMoveController>();
                    moveController.SetEnemyData(enemyData);
                    moveController.SetSpeedByWave(waveNumber);

                    // Đợi 1 giây trước khi sinh ra enemy tiếp theo
                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                Debug.LogWarning($"[WaveManager] Không tìm thấy dữ liệu wave {waveNumber} cho enemy {enemyData.enemyName}");
            }
        }
    }

    public void SetMapData(Map newMapData)
    {
        mapData = newMapData;
    }
}