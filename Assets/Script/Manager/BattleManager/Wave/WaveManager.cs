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
    public Action OnWaveSpawnComplete;   // toàn bộ enemy đã spawn xong
    public Action OnWaveCleared;         // toàn bộ enemy đã chết

    private int _aliveEnemyCount;

    private void Awake()
    {
        currentWaveNumber = 1;
        cts = new CancellationTokenSource();
    }

    public void Init(Map map, WayPointForEnemy wayPoint)
    {
        mapData = map;
        enemyBattleDatas = map.enemyDatas;
        wayPointSystem = wayPoint;

        if (wayPointSystem == null)
            Debug.LogError("[WaveManager] Không tìm thấy WayPointForEnemy trong scene!");
        else if (wayPointSystem.wayPoints.Count == 0)
            Debug.LogError("[WaveManager] WayPointForEnemy không có waypoint nào!");

        // Preload enemy vào pool từ BattleAssetManager
        if (EnemyObjectPool.Instance != null)
            foreach (var enemyData in enemyBattleDatas)
            {
                var prefab = BattleAssetManager.Instance?.GetEnemyPrefab(enemyData.id);
                if (prefab != null)
                    EnemyObjectPool.Instance.RegisterEnemy(enemyData.enemyName.Trim(), prefab);
            }
    }

public void StartBattle()
    {
        if (mapData == null || wayPointSystem == null)
        {
            Debug.LogError("[WaveManager] Chưa được Init trước khi StartBattle!");
            return;
        }
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

        // Đếm tổng enemy sẽ spawn trong wave này
        int totalCount = 0;
        foreach (var ed in enemyBattleDatas)
        {
            var wd = ed.getEnemyWaveDataByName(waveNumber);
            if (wd != null) totalCount += wd.enemyStats.count;
        }
        _aliveEnemyCount = totalCount;

        // Không còn enemy nào để spawn — tất cả wave đã xong
        if (totalCount == 0)
        {
            MatchTracker.Instance?.NotifyAllWavesComplete();
            return;
        }

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
                var enemyPrefab = BattleAssetManager.Instance?.GetEnemyPrefab(enemyData.id);
                if (enemyPrefab == null) { _aliveEnemyCount--; continue; }

                GameObject enemyInstance = EnemyObjectPool.Instance != null
                    ? EnemyObjectPool.Instance.GetOrRegister(enemyData.enemyName, enemyPrefab, spawnPoint.position, Quaternion.identity)
                    : Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

                if (enemyInstance == null) { _aliveEnemyCount--; continue; }

                MatchTracker.Instance?.RegisterEnemySpawned();

                EnemyMoveController moveController = enemyInstance.GetComponent<EnemyMoveController>();
                moveController.SetEnemyData(enemyData);
                moveController.SetSpeedByWave(waveNumber);
                moveController.ResetForReuse(wayPointSystem);

                EnemyHPController hpController = enemyInstance.GetComponent<EnemyHPController>();
                hpController.SetEnemyData(enemyData);
                hpController.SetPlayerStats(playerStatsInBattleManager, waveNumber);
                hpController.setHpByWaveName(waveNumber);
                hpController.OnDied += OnEnemyDied;
                hpController.PlaySpawnSFX();

                await UniTask.Delay(1000, cancellationToken: ct);
            }
        }

        // Toàn bộ enemy đã được spawn — show next wave button
        OnWaveSpawnComplete?.Invoke();
    }

    private void OnEnemyDied()
    {
        _aliveEnemyCount--;
        if (_aliveEnemyCount <= 0)
        {
            OnWaveCleared?.Invoke();
            StartNextWave();
        }
    }

}
