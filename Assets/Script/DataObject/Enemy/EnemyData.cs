using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "PokeSoul/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string id;
    public string enemyName;
    public Sprite spriteEnemy;
    
    [Header("Thông tin chiến đấu")]
    public AssetReference enemyPrefabRef;
    public List<EnemyWaveData> enemyWaves;

    public EnemyWaveData getEnemyWaveDataByName(int waveName)
    {
        foreach (var wave in enemyWaves)
        {
            if (wave.waveName == waveName)
            {
                return wave;
            }
        }
        Debug.LogWarning($"[EnemyData] Không tìm thấy EnemyWaveData với tên: {waveName}");
        return null;
    }

}

[System.Serializable]
public class EnemyStats
{
    public int count;
    public double HP;
    public double speed;
    public int reward;
    
}

[System.Serializable]
public class EnemyWaveData
{
    public int waveName;
    public EnemyStats enemyStats;
}