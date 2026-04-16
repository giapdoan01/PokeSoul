using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public int myGem;
    public AllPokemonData allPokemonData;
    private PokemonData[] myPokemonDatas;


    void Start()
    {
      
    }
    public async void BuyPokemon(PokemonData pokemonData, int price)
    {
        if (PlayerDataManager.Instance.CurrentData.gem >= price)
        {
            await PlayerDataManager.Instance.MinusGemAsync(price);
            await PlayerDataManager.Instance.AddCardToOwnCardAsync(pokemonData.id);
            Debug.Log($"[ShopManager] Đã mua {pokemonData.PokemonName} với giá {price} gem. Gem còn lại: {PlayerDataManager.Instance.CurrentData.gem}");
        }
        else
        {
            Debug.LogWarning($"[ShopManager] Không đủ gem để mua {pokemonData.PokemonName}. Gem hiện tại: {PlayerDataManager.Instance.CurrentData.gem}");
        }
    }
    public void SetupMyPokemonData(PokemonData[] pokemonDatas)
    {
        myPokemonDatas = pokemonDatas;
    }
}