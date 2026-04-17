using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDeckHomePageItemPrefab : MonoBehaviour
{
    public Image backgroundCardWhenNoData;
    public Image typeCardBackgroundImage;
    public Image monImage;
    public TMP_Text monName;
    public TMP_Text typeText;
    public Image TypeImage;
    public List<Sprite> TypeSprites;
    public List<Sprite> typeCardBackgroundImageList;
    public void SetupCard(PokemonData pokemonData)
    {
        backgroundCardWhenNoData.enabled = false;
        monImage.sprite = pokemonData.spritePokemonCard;
        monName.text = pokemonData.PokemonName;
        typeText.text = pokemonData.type.ToString();
        SetupType(pokemonData);
    }
    public void SetupEmptyCard()
    {
        backgroundCardWhenNoData.enabled = true;
        typeCardBackgroundImage.enabled = false;
        monImage.enabled = false;
        monName.text = "Add Mon";
        typeText.text = "";
        TypeImage.enabled = false;
    }

    public void SetupType(PokemonData cardPokemon)
    {
        switch (cardPokemon.type)
        {
            case PokemonType.Fire:
                TypeImage.sprite = TypeSprites[0];
                typeCardBackgroundImage.sprite = typeCardBackgroundImageList[0];
                break;
            case PokemonType.Water:
                TypeImage.sprite = TypeSprites[1];
                typeCardBackgroundImage.sprite = typeCardBackgroundImageList[1];
                break;
            case PokemonType.Grass:
                TypeImage.sprite = TypeSprites[2];
                typeCardBackgroundImage.sprite = typeCardBackgroundImageList[2];
                break;
            case PokemonType.Electric:
                TypeImage.sprite = TypeSprites[3];
                typeCardBackgroundImage.sprite = typeCardBackgroundImageList[3];
                break;
            case PokemonType.Psychic:
                TypeImage.sprite = TypeSprites[4];
                typeCardBackgroundImage.sprite = typeCardBackgroundImageList[4];
                break;
            case PokemonType.Ice:
                TypeImage.sprite = TypeSprites[5];
                typeCardBackgroundImage.sprite = typeCardBackgroundImageList[5];
                break;
            case PokemonType.Dark:
                TypeImage.sprite = TypeSprites[6];
                typeCardBackgroundImage.sprite = typeCardBackgroundImageList[6];
                break;
            case PokemonType.Fighting:
                TypeImage.sprite = TypeSprites[7];
                typeCardBackgroundImage.sprite = typeCardBackgroundImageList[7];
                break;
            case PokemonType.Poison:
                TypeImage.sprite = TypeSprites[8];
                typeCardBackgroundImage.sprite = typeCardBackgroundImageList[8];
                break;
            case PokemonType.Ground:
                TypeImage.sprite = TypeSprites[9];
                typeCardBackgroundImage.sprite = typeCardBackgroundImageList[9];
                break;
        }
    }
}