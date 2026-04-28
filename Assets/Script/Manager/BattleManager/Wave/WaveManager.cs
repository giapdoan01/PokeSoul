using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

public class WaveManager : MonoBehaviour
{
    public Map mapData;
    public PlayerStatsInBattleManager playerStatsInBattleManager;
    public int timeToStartFirstWave = 10;

    private int currentWaveNumber;
    private EnemyData[] enemyBattleDatas;
    private WayPointForEnemy wayPointSystem;
    private CancellationTokenSource cts;

    public Action<int> OnCountdownTick;
    public Action OnCountdownFinished;

    private void Awake()
    {
        if (mapData == null)
        {
            Debug.LogError("[WaveManager] Không tìm thấy Map Data! Vui lòng gán Map Data trong Inspector.");
            return;
        }

        currentWaveNumber = 1;
        enemyBattleDatas = mapData.enemyDatas;
        wayPointSystem = FindObjectOfType<WayPointForEnemy>();

        if (wayPointSystem == null)
            Debug.LogError("[WaveManager] Không tìm thấy WayPointForEnemy trong scene!");
        else if (wayPointSystem.wayPoints.Count == 0)
            Debug.LogError("[WaveManager] WayPointForEnemy không có waypoint nào!");
    }

    private void Start()
    {
        cts = new CancellationTokenSource();
        StartNextWave();
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

    public void StartNextWave()
    {
        Debug.Log($"[WaveManager] Bắt đầu wave {currentWaveNumber}");
        SpawnEnemiesForCurrentWave(currentWaveNumber, cts.Token).Forget();
        currentWaveNumber++;
    }

    private async UniTaskVoid SpawnEnemiesForCurrentWave(int waveNumber, CancellationToken ct)
    {
        WaveReward waveReward = mapData.getWaveRewardByWaveNumber(waveNumber);
        if (waveReward != null)
        {
            int totalReward = waveReward.waveReward + waveReward.waveSpecialReward;
            playerStatsInBattleManager.AddCoin(totalReward);
        }
        else
        {
            Debug.LogWarning($"[WaveManager] Không tìm thấy phần thưởng cho wave {waveNumber}");
        }

        if (waveNumber == 1)
        {
            for (int t = timeToStartFirstWave; t > 0; t--)
            {
                OnCountdownTick?.Invoke(t);
                await UniTask.Delay(1000, cancellationToken: ct);
            }
            OnCountdownFinished?.Invoke();
        }

        if (wayPointSystem == null || wayPointSystem.wayPoints.Count == 0)
        {
            Debug.LogError("[WaveManager] Không thể spawn enemy vì không tìm thấy waypoint!");
            return;
        }

        Transform spawnPoint = wayPointSystem.wayPoints[0];

        foreach (var enemyData in enemyBattleDatas)
        {
            EnemyWaveData waveData = enemyData.getEnemyWaveDataByName(waveNumber);
            if (waveData == null)
            {
                Debug.LogWarning($"[WaveManager] Không tìm thấy dữ liệu wave {waveNumber} cho enemy {enemyData.enemyName}");
                continue;
            }

            for (int i = 0; i < waveData.enemyStats.count; i++)
            {
                GameObject enemyInstance = Instantiate(enemyData.enemyPrefab, spawnPoint.position, Quaternion.identity);

                EnemyHPController hpController = enemyInstance.GetComponent<EnemyHPController>();
                hpController.SetEnemyData(enemyData);
                hpController.setHpByWaveName(waveNumber);

                EnemyMoveController moveController = enemyInstance.GetComponent<EnemyMoveController>();
                moveController.SetEnemyData(enemyData);
                moveController.SetSpeedByWave(waveNumber);

                await UniTask.Delay(1000, cancellationToken: ct);
            }
        }
    }

    public void SetMapData(Map newMapData)
    {
        mapData = newMapData;
    }
}
