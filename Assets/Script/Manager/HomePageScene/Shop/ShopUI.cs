using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public ShopManager shopManager;
    public Transform cardContainer;
    public GameObject cardShopItemPrefab;

    void Start()
    {
        if (shopManager == null)
            Debug.LogError("[ShopUI] Không tìm thấy ShopManager trong scene!");

        SetupShop();
    }

    public void SetupShop()
    {
        if (cardContainer == null || cardShopItemPrefab == null || shopManager == null) return;

        // Xóa các item cũ trước khi spawn mới
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        foreach (var pokemonData in shopManager.allPokemonData.allPokemonDatas)
        {
            bool isPurchased = shopManager.IsOwned(pokemonData.id);
            var item = Instantiate(cardShopItemPrefab, cardContainer).GetComponent<CardShopItemPrefab>();
            item.SetupCardShopItem(pokemonData, pokemonData.priceToBuyCard, isPurchased, shopManager);
        }
    }
}
