using UnityEngine;
using System;

public class ShopManager : MonoBehaviour
{
    public AllPokemonData allPokemonData;
    private PokemonData[] myPokemonDatas;

    void Start() { }

    public async void BuyPokemon(PokemonData pokemonData, int price, Action<PokemonData> onSuccess = null, PopupNotificationShop popup = null)
    {
        if (PlayerDataManager.Instance.CurrentData.gem < price)
        {
            Debug.LogWarning($"[ShopManager] Không đủ gem để mua {pokemonData.PokemonName}. Gem hiện tại: {PlayerDataManager.Instance.CurrentData.gem}");
            if (popup != null)
            {
                popup.gameObject.SetActive(true);
                popup.SetupNotificationNotEnoughGem("Not Enough Gem");
            }
            return;
        }

        bool success = await LoadingManager.Instance.RunWithLoadingAsync(async () =>
        {
            await PlayerDataManager.Instance.MinusGemAsync(price);
            await PlayerDataManager.Instance.AddCardToOwnCardAsync(pokemonData.id);
        }, $"Đang mua {pokemonData.PokemonName}...");

        if (success)
        {
            Debug.Log($"[ShopManager] Đã mua {pokemonData.PokemonName} với giá {price} gem. Gem còn lại: {PlayerDataManager.Instance.CurrentData.gem}");
            onSuccess?.Invoke(pokemonData);
            if (popup != null)
            {
                popup.gameObject.SetActive(true);
                popup.SetupNotificationBuyMonSuccess(pokemonData, $"Purchase Successful!\n{pokemonData.PokemonName}");
            }
        }
    }

    public void SetupMyPokemonData(PokemonData[] pokemonDatas)
    {
        myPokemonDatas = pokemonDatas;
    }
    public void SetUpAllPokemonData(AllPokemonData data)
    {
        allPokemonData = data;
    }

    public bool IsOwned(string cardId)
    {
        return PlayerDataManager.Instance.CurrentData.ownCard.Contains(cardId);
    }
}
