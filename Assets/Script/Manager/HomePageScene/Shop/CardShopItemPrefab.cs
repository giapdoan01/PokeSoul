using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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
    public PopupNotificationShop popupNotificationShop;
    public List<Sprite> typeSprites;
    public List<Sprite> typeCardBackgroundSprites;

    [Header("Color Text By Type")]
    public Color fireColor = new Color(0f, 0f, 0f);
    public Color waterColor = new Color(0f, 0f, 0f);
    public Color grassColor = new Color(0f, 0f, 0f);
    public Color electricColor = new Color(0f, 0f, 0f);
    public Color psychicColor = new Color(0f, 0f, 0f);
    public Color iceColor = new Color(0f, 0f, 0f);
    public Color darkColor = new Color(0f, 0f, 0f);
    public Color fightingColor = new Color(0f, 0f, 0f);
    public Color poisonColor = new Color(0f, 0f, 0f);
    public Color groundColor = new Color(0f, 0f, 0f);

    [Header("SFX")]
    [SerializeField] private AudioClip buttonClickSFX;

    private PokemonData _pokemonData;
    private int _price;
    private ShopManager _shopManager;
    private Vector3 _buyButtonOriginalScale;

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
        SetupTextColor(pokemonData.type);
        SetPurchasedState(isPurchased);

        _buyButtonOriginalScale = buyButton.transform.localScale;

        buyButton.GetComponent<Button>().onClick.RemoveAllListeners();
        buyButton.GetComponent<Button>().onClick.AddListener(OnBuyButtonClicked);
    }

    // Gắn vào Button OnClick trong Inspector
    public void OnBuyButtonClicked()
    {
        SoundUIManager.Instance?.PlayUISound(buttonClickSFX);
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
        StartCoroutine(PressAnimation());
        _shopManager.BuyPokemon(_pokemonData, _price, OnBuySuccess, popupNotificationShop).Forget();
    }

    private IEnumerator PressAnimation()
    {
        buyButton.transform.localScale = _buyButtonOriginalScale * 0.88f;
        yield return new WaitForSeconds(0.12f);
        buyButton.transform.localScale = _buyButtonOriginalScale;
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

    private void SetupTextColor(PokemonType type)
    {
        Color color = type switch
        {
            PokemonType.Fire     => fireColor,
            PokemonType.Water    => waterColor,
            PokemonType.Grass    => grassColor,
            PokemonType.Electric => electricColor,
            PokemonType.Psychic  => psychicColor,
            PokemonType.Ice      => iceColor,
            PokemonType.Dark     => darkColor,
            PokemonType.Fighting => fightingColor,
            PokemonType.Poison   => poisonColor,
            PokemonType.Ground   => groundColor,
            _                    => Color.white
        };

        pokemonNameText.color = color;
        typeNameText.color = color;
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
