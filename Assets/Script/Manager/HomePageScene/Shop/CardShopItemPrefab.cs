using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class CardShopItemPrefab : MonoBehaviour
{
    public Image typeCardBackground;
    public Image pokemonImage;
    public TMP_Text pokemonNameText;
    public Image typeImage;
    public TMP_Text typeNameText;
    public GameObject buyButton;
    public TMP_Text priceText;
    public Image gemIcon;
    public Image purchasedImage;
    public List<Sprite> typeSprites;
    public List<Sprite> typeCardBackgroundSprites;

    private PokemonData _pokemonData;
    private int _price;
    private ShopManager _shopManager;

    public void SetupCardShopItem(PokemonData pokemonData, int price, bool isPurchased, ShopManager shopManager)
    {
        _pokemonData = pokemonData;
        _price = price;
        _shopManager = shopManager;

        pokemonImage.sprite = pokemonData.spritePokemonCard;
        pokemonNameText.text = pokemonData.PokemonName;
        typeNameText.text = pokemonData.type.ToString();
        priceText.text = price.ToString();
        priceText.gameObject.SetActive(!isPurchased);

        SetupType(pokemonData);
        SetPurchasedState(isPurchased);

        buyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        buyButton.GetComponent<Button>().onClick.AddListener(OnBuyButtonClicked);
    }

    // Gắn vào Button OnClick trong Inspector
    public void OnBuyButtonClicked()
    {
        if (_pokemonData == null)
        {
            Debug.LogError("[CardShopItemPrefab] _pokemonData null — SetupCardShopItem chưa được gọi!");
            return;
        }
        if (_shopManager == null)
        {
            Debug.LogError("[CardShopItemPrefab] _shopManager null — SetupCardShopItem chưa được gọi!");
            return;
        }
        _shopManager.BuyPokemon(_pokemonData, _price, OnBuySuccess);
    }

    private void OnBuySuccess(PokemonData _)
    {
        SetPurchasedState(true);
    }

    private void SetPurchasedState(bool isPurchased)
    {
        var btn = buyButton.GetComponent<Button>();
        btn.interactable = !isPurchased;
        gemIcon.gameObject.SetActive(!isPurchased);
        priceText.gameObject.SetActive(!isPurchased);
        purchasedImage.gameObject.SetActive(isPurchased);
    }

    private void SetupType(PokemonData pokemonData)
    {
        int index = pokemonData.type switch
        {
            PokemonType.Fire => 0,
            PokemonType.Water => 1,
            PokemonType.Grass => 2,
            PokemonType.Electric => 3,
            PokemonType.Psychic => 4,
            PokemonType.Ice => 5,
            PokemonType.Dark => 6,
            PokemonType.Fighting => 7,
            PokemonType.Poison => 8,
            PokemonType.Ground => 9,
            _ => -1
        };

        if (index >= 0 && index < typeSprites.Count)
            typeImage.sprite = typeSprites[index];

        if (index >= 0 && index < typeCardBackgroundSprites.Count)
            typeCardBackground.sprite = typeCardBackgroundSprites[index];
    }
}
