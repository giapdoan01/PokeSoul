using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerStatsInBattleManager : MonoBehaviour
{
    public int playerCoin;
    private PokemonData[] pokemonDatas;
    public Action<int> OnCoinChanged;
    void Awake() 
    {
        
    }

    public void AddCoin(int amount)
    {
        playerCoin += amount;
        Debug.Log($"[PlayerStatsManager] Đã thêm {amount} coin. Tổng coin hiện tại: {playerCoin}");
        OnCoinChanged?.Invoke(playerCoin);
    }
    public void MinusCoin(int amount)
    {
        if (playerCoin >= amount)
        {
            playerCoin -= amount;
            Debug.Log($"[PlayerStatsManager] Đã trừ {amount} coin. Tổng coin hiện tại: {playerCoin}");
        }
        else
        {
            Debug.LogWarning($"[PlayerStatsManager] Không đủ coin để trừ. Tổng coin hiện tại: {playerCoin}");
            return;
        }
        OnCoinChanged?.Invoke(playerCoin);
    }
    public void SetPokemonDatas(PokemonData[] datas)
    {
        pokemonDatas = datas;
    }
    public PokemonData[] GetPokemonDatas()
    {
        return pokemonDatas;
    }
}