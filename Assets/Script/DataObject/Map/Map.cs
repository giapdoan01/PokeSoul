using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NewMapData", menuName = "PokeSoul/Map Data")]
public class Map : ScriptableObject
{
    public string id;
    public string mapName;
    public Sprite mapSprite;
    public EnemyData[] enemyDatas;
    public WaveData[] waves;

    public WaveReward getWaveRewardByWaveNumber(int waveNumber)
    {
        foreach (var wave in waves)
        {
            if (wave.waveNumber == waveNumber)
            {
                return wave.waveReward;
            }
        }
        Debug.LogWarning($"[Map] Không tìm thấy WaveReward với số wave: {waveNumber}");
        return null;
    }

}

[System.Serializable]
public class WaveData
{
    public int waveNumber;
    public WaveReward waveReward;
}

[System.Serializable]
public class WaveReward
{
    public int waveReward;
    public int waveSpecialReward;
}