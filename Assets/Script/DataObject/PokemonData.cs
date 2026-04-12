using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class StatEntry
{
    public string statName;
    public double value;
}
[CreateAssetMenu(fileName = "NewPokemonData", menuName = "PokeSoul/Pokemon Data")]
public class PokemonData : ScriptableObject
{
    public GameObject pokemonPrefab;
    public GameObject skillPrefab;
    public double hp;
    public double attack;
    public double cooldownTimeSkill;
    public StatEntry[] statEntries;

}
