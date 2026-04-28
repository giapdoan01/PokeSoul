using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class ShopManager : MonoBehaviour
{
    public AllPokemonData allPokemonData;
    private PokemonData[] myPokemonDatas;

    public async UniTaskVoid BuyPokemon(PokemonData pokemonData, int price, Action<PokemonData> onSuccess = null, PopupNotificationShop popup = null)
    {
        if (PlayerDataManager.Instance.CurrentData.gem < price)
        {
            Debug.LogWarning($"[ShopManager] Không đủ gem để mua {pokemonData.PokemonName}.");
            popup?.SetupNotificationNotEnoughGem("Not Enough Gem");
            return;
        }

        bool success = await LoadingManager.Instance.RunWithLoadingAsync(async () =>
        {
            await PlayerDataManager.Instance.MinusGemAsync(price);
            await PlayerDataManager.Instance.AddCardToOwnCardAsync(pokemonData.id);
        }, $"Đang mua {pokemonData.PokemonName}...");

        if (!success) return;

        Debug.Log($"[ShopManager] Đã mua {pokemonData.PokemonName} với giá {price} gem.");
        onSuccess?.Invoke(pokemonData);
        popup?.SetupNotificationBuyMonSuccess(pokemonData, $"Purchase Successful!\n{pokemonData.PokemonName}");
    }

    public bool IsOwned(string cardId) =>
        PlayerDataManager.Instance.CurrentData.ownCard.Contains(cardId);

    public void SetupMyPokemonData(PokemonData[] pokemonDatas) => myPokemonDatas = pokemonDatas;
    public void SetUpAllPokemonData(AllPokemonData data) => allPokemonData = data;
}
