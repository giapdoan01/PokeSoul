using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class PlayerStatsInBattleUI : MonoBehaviour
{
    public Text coinText;
    public PlayerStatsInBattleManager playerStatsInBattleManager;
    private PokemonData[] pokemonDatas;

    void Awake()
    {
        if (playerStatsInBattleManager == null)
        {
            Debug.LogError("[PlayerStatsInBattleUI] Không tìm thấy PlayerStatsInBattleManager trong scene!");
        }
        playerStatsInBattleManager.OnCoinChanged += UpdateCoinText;
    }
    void Start()
    {
        if (playerStatsInBattleManager != null)
        {
            pokemonDatas = playerStatsInBattleManager.GetPokemonDatas();
            if (pokemonDatas == null || pokemonDatas.Length == 0)
            {
                Debug.LogWarning("[PlayerStatsInBattleUI] PlayerStatsInBattleManager không có PokemonData nào!");
            }
        }
    }

    void UpdateCoinText(int coin)
    {
        if (coinText != null)
        {
            coinText.text = $"Coin: {coin}";
        }
    }

}