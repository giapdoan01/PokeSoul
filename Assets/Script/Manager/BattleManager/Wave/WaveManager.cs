using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

public class WaveManager : MonoBehaviour
{
    public Map mapData;
    public PlayerStatsInBattleManager playerStatsInBattleManager;
    public int timeToStartFirstWave = 10;

    private MapInfoDisplay _mapInfoDisplay;

    private int currentWaveNumber;
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
        wayPointSystem = wayPoint;

        if (wayPointSystem == null)
            Debug.LogError("[WaveManager] Không tìm thấy WayPointForEnemy trong scene!");
        else if (wayPointSystem.wayPoints.Count == 0)
            Debug.LogError("[WaveManager] WayPointForEnemy không có waypoint nào!");

        // Preload enemy vào pool từ BattleAssetManager (lấy từ spawnSequence)
        if (EnemyObjectPool.Instance != null)
            foreach (var wave in map.waves)
                if (wave.spawnSequence != null)
                    foreach (var entry in wave.spawnSequence)
                    {
                        if (entry.enemyData == null) continue;
                        var prefab = BattleAssetManager.Instance?.GetEnemyPrefab(entry.enemyData.id);
                        if (prefab != null)
                            EnemyObjectPool.Instance.RegisterEnemy(entry.enemyData.enemyName.Trim(), prefab);
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

    public void SetMapInfoDisplay(MapInfoDisplay display)
    {
        _mapInfoDisplay = display;
        RefreshWaveDisplay();
    }

    public void StartNextWave()
    {
        Debug.Log($"[WaveManager] Bắt đầu wave {currentWaveNumber}");
        SpawnEnemiesForCurrentWave(currentWaveNumber, cts.Token).Forget();
        currentWaveNumber++;
        RefreshWaveDisplay();
    }

    private void RefreshWaveDisplay()
    {
        int total = mapData?.waves?.Length ?? 0;
        _mapInfoDisplay?.UpdateWave(currentWaveNumber, total);
    }

    private async UniTaskVoid SpawnEnemiesForCurrentWave(int waveNumber, CancellationToken ct)
    {
        WaveReward waveReward = mapData.getWaveRewardByWaveNumber(waveNumber);
        if (waveReward != null)
            playerStatsInBattleManager.AddCoin(waveReward.waveReward + waveReward.waveSpecialReward);
        else
            Debug.LogWarning($"[WaveManager] Không tìm thấy phần thưởng cho wave {waveNumber}");

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

        WaveData waveData = GetWaveData(waveNumber);

        if (waveData?.spawnSequence != null && waveData.spawnSequence.Count > 0)
            await SpawnBySequence(waveData.spawnSequence, ct);
        else
        {
            MatchTracker.Instance?.NotifyAllWavesComplete();
            return;
        }

        OnWaveSpawnComplete?.Invoke();
    }

    // Spawn theo spawnSequence mới — hỗ trợ thứ tự mixed enemy
    private async UniTask SpawnBySequence(List<WaveSpawnEntry> sequence, CancellationToken ct)
    {
        Transform spawnPoint = wayPointSystem.wayPoints[0];

        int totalCount = 0;
        foreach (var entry in sequence)
            if (entry.enemyData != null) totalCount += entry.count;
        _aliveEnemyCount = totalCount;

        if (totalCount == 0)
        {
            MatchTracker.Instance?.NotifyAllWavesComplete();
            return;
        }

        foreach (var entry in sequence)
        {
            if (entry.enemyData == null) continue;

            for (int i = 0; i < entry.count; i++)
            {
                bool spawned = await SpawnSingleEnemy(entry, spawnPoint.position, ct);
                if (!spawned) _aliveEnemyCount--;

                float delay = Mathf.Max(0.1f, entry.delayBetweenSpawns);
                await UniTask.Delay((int)(delay * 1000), cancellationToken: ct);
            }
        }
    }

    private async UniTask<bool> SpawnSingleEnemy(WaveSpawnEntry entry, Vector3 spawnPos, CancellationToken ct)
    {
        var enemyData = entry.enemyData;
        var enemyPrefab = BattleAssetManager.Instance?.GetEnemyPrefab(enemyData.id);
        if (enemyPrefab == null) return false;

        GameObject enemyInstance = EnemyObjectPool.Instance != null
            ? EnemyObjectPool.Instance.GetOrRegister(enemyData.enemyName, enemyPrefab, spawnPos, Quaternion.identity)
            : Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        if (enemyInstance == null) return false;

        MatchTracker.Instance?.RegisterEnemySpawned();

        EnemyMoveController moveController = enemyInstance.GetComponent<EnemyMoveController>();
        moveController.SetEnemyData(enemyData);
        moveController.SetSpeed(entry.speed);
        moveController.ResetForReuse(wayPointSystem);

        EnemyHPController hpController = enemyInstance.GetComponent<EnemyHPController>();
        hpController.SetEnemyData(enemyData);
        hpController.SetPlayerStats(playerStatsInBattleManager, entry.reward);
        hpController.SetHp(entry.hp);
        hpController.OnDied += OnEnemyDied;
        hpController.PlaySpawnSFX();

        return true;
    }

    private WaveData GetWaveData(int waveNumber)
    {
        if (mapData.waves == null) return null;
        foreach (var w in mapData.waves)
            if (w.waveNumber == waveNumber) return w;
        return null;
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
