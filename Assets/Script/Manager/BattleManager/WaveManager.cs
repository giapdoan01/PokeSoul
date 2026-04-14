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

    void Awake() 
    {
        if (mapData == null)
        {
            Debug.LogError("[WaveManager] Không tìm thấy Map Data! Vui lòng gán Map Data trong Inspector.");
            return;
        }
        currentWaveNumber = 1;
        enemyBattleDatas = mapData.enemyDatas;
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
        SpawnEnemiesForCurrentWave();
        currentWaveNumber++;
    }
    private void SpawnEnemiesForCurrentWave()
    {
        foreach (var enemyData in enemyBattleDatas)
        {
            EnemyWaveData waveData = enemyData.getEnemyWaveDataByName(currentWaveNumber);
            if (waveData != null)
            {
                for (int i = 0; i < waveData.enemyStats.count; i++)
                {
                    Instantiate(enemyData.enemyPrefab, transform.position, Quaternion.identity);
                    enemyData.enemyPrefab.GetComponent<EnemyHPController>().setHpByWaveName(currentWaveNumber);
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