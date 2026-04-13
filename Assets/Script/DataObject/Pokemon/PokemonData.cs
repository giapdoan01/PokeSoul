using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(fileName = "NewPokemonData", menuName = "PokeSoul/Pokemon Data")]
public class PokemonData : ScriptableObject
{
    [Header("InfoCard")]
    public string id;
    public string PokemonName;
    public PokemonType type;
    public Sprite spritePokemonCard;
    public int priceToBuyCard;

    [Header("Info In battle")]
    public GameObject pokemonPrefab;
    public GameObject skillPrefab;
    public PokemonData EvolutionPokemonData;
    public PokemonLevelData[] levelUpData;

    public StatEntry[] getStatEntryByLevel(int level)
    {
        foreach (var levelData in levelUpData)
        {
            if (levelData.level == level)
                return levelData.statEntries;
        }
        Debug.LogWarning($"[PokemonData] Không tìm thấy level: {level}");
        return null;
    }
}

public enum PokemonType
{
    Fire,
    Water,
    Grass,
    Electric,
    Psychic,
    Ice,
    Dark,
    Fighting,
    Poison,
    Ground
}

[System.Serializable]
public class StatEntry
{
    public string statName;
    public double value;

    // ── Properties ──
    public string StatName
    {
        get { return statName; }
        set { statName = value; }
    }

    public double Value
    {
        get { return value; }
        set { this.value = value < 0 ? 0 : value; } 
    }

    // ── Constructor ──
    public StatEntry() { }

    public StatEntry(string name, double val)
    {
        statName = name;
        value    = val;
    }
    public double GetValue()
    {
        return value;
    }
}

[System.Serializable]
public class PokemonLevelData
{
    public int level;
    public StatEntry[] statEntries;
}

