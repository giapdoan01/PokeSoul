using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AllPokemonData", menuName = "PokeSoul/All Pokemon Data")]
public class AllPokemonData : ScriptableObject
{
    public PokemonData[] allPokemonDatas;
    public PokemonData GetPokemonDataById(string id)
    {
        foreach (var data in allPokemonDatas)
        {
            if (data.id == id)
                return data;
        }
        Debug.LogWarning($"[AllPokemonData] Không tìm thấy PokemonData với id: {id}");
        return null;
    }
}