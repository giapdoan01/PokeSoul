using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    public Text coinText;
    public PlayerStatsManager playerStatsManager;
    private PokemonData[] pokemonDatas;

    void Awake()
    {
        if (playerStatsManager == null)
        {
            Debug.LogError("[PlayerStatsUI] Không tìm thấy PlayerStatsManager trong scene!");
        }
        playerStatsManager.OnCoinChanged += UpdateCoinText;
    }
    void Start()
    {
        if (playerStatsManager != null)
        {
            pokemonDatas = playerStatsManager.GetPokemonDatas();
            if (pokemonDatas == null || pokemonDatas.Length == 0)
            {
                Debug.LogWarning("[PlayerStatsUI] PlayerStatsManager không có PokemonData nào!");
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