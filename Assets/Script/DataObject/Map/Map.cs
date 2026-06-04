using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NewMapData", menuName = "PokeSoul/Map Data")]
public class Map : ScriptableObject
{
    public string id;
    public string mapName;
    public Sprite mapSprite;
    public AssetReference mapPrefabRef;
    public int rewardWinMap;
    public WaveData[] waves;

    public WaveReward getWaveRewardByWaveNumber(int waveNumber)
    {
        foreach (var wave in waves)
        {
            if (wave.waveNumber == waveNumber)
                return wave.waveReward;
        }
        Debug.LogWarning($"[Map] Không tìm thấy WaveReward với số wave: {waveNumber}");
        return null;
    }
}


[System.Serializable]
public class MapProgress
{
    public string mapId;
    public bool isCompleted;

    public MapProgress(string id)
    {
        mapId = id;
        isCompleted = false;
    }
}

[System.Serializable]
public class WaveSpawnEntry
{
    public EnemyData enemyData;
    public int count = 1;
    public float delayBetweenSpawns = 1f;
    public double hp = 100;
    public double speed = 3;
    public int reward = 10;
}

[System.Serializable]
public class WaveData
{
    public int waveNumber;
    public WaveReward waveReward;
    public List<WaveSpawnEntry> spawnSequence;
}

[System.Serializable]
public class WaveReward
{
    public int waveReward;
    public int waveSpecialReward;
}