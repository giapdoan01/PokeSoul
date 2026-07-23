using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CardShopItemPrefab : MonoBehaviour
{
    public Image typeCardBackground;
    public Image pokemonImage;
    public TMP_Text pokemonNameText;
    public Image typeImage;
    public Image RarityImage;
    public GameObject ownedOverlay;
    public GameObject frameOn;
    public List<Sprite> typeSprites;
    public List<Sprite> typeCardBackgroundSprites;
    public List<Sprite> RaritySprites;

    [Header("Color Text By Type")]
    public Color fireColor;
    public Color waterColor;
    public Color grassColor;
    public Color electricColor;
    public Color psychicColor;
    public Color iceColor;
    public Color darkColor;
    public Color fightingColor;
    public Color poisonColor;
    public Color groundColor;

    public PokemonData PokemonData { get; private set; }
    public bool IsOwned { get; private set; }

    public void Setup(PokemonData data, bool isOwned)
    {
        PokemonData = data;

        pokemonImage.sprite = data.spritePokemonCard;
        pokemonNameText.text = data.PokemonName;

        SetupType(data);
        SetupTextColor(data.type);
        SetupRarity(data);
        RefreshOwnedState(isOwned);

        frameOn?.SetActive(false);
    }

    public void RefreshOwnedState(bool isOwned)
    {
        IsOwned = isOwned;
        ownedOverlay?.SetActive(isOwned);
    }

    public void SetHighlight(bool active)
    {
        if (!IsOwned)
            frameOn?.SetActive(active);
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
    }

    private void SetupType(PokemonData data)
    {
        int index = data.type switch
        {
            PokemonType.Fire     => 0,
            PokemonType.Water    => 1,
            PokemonType.Grass    => 2,
            PokemonType.Electric => 3,
            PokemonType.Psychic  => 4,
            PokemonType.Ice      => 5,
            PokemonType.Dark     => 6,
            PokemonType.Fighting => 7,
            PokemonType.Poison   => 8,
            PokemonType.Ground   => 9,
            _                    => -1
        };

        if (index >= 0 && index < typeSprites.Count)
            typeImage.sprite = typeSprites[index];

        if (index >= 0 && index < typeCardBackgroundSprites.Count)
            typeCardBackground.sprite = typeCardBackgroundSprites[index];
    }
    public void SetupRarity(PokemonData data)
    {
        int index = data.Rarity switch
        {
            "C"    => 0,
            "R"  => 1,
            "S"      => 2,
            "SSR"      => 3,
            "SSS" => 4,
            _           => -1
        };

        if (index >= 0 && index < RaritySprites.Count)
            RarityImage.sprite = RaritySprites[index];
    }
}
