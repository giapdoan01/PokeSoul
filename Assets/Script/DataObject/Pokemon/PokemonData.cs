using UnityEngine;
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

    public PokemonLevelData getPokemonLevelDataByLevel(int level)
    {
        foreach (var levelData in levelUpData)
        {
            if (levelData.level == level)
            {
                return levelData;
            }
        }

        Debug.LogWarning($"[PokemonData] Khong tim thay level: {level}");
        return null;
    }

    public bool TryGetStatValueByLevel(int level, string statName, out double value)
    {
        value = 0;

        PokemonLevelData levelData = getPokemonLevelDataByLevel(level);
        if (levelData == null)
        {
            return false;
        }

        StatEntry entry = levelData.GetStatEntryByName(statName);
        if (entry == null)
        {
            return false;
        }

        value = entry.Value;
        return true;
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

    public StatEntry() { }

    public StatEntry(string name, double val)
    {
        statName = name;
        value = val;
    }
}

[System.Serializable]
public class PokemonLevelData
{
    public int level;
    public StatEntry[] statEntries;

    public StatEntry GetStatEntryByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        foreach (var entry in statEntries)
        {
            if (string.Equals(entry.StatName, name, StringComparison.OrdinalIgnoreCase))
            {
                return entry;
            }
        }

        return null;
    }
}
