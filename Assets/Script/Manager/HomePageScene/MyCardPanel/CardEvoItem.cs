using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class CardEvoItem : MonoBehaviour
{
    public Image TypeCardBackground;
    public Image MonImage;
    public TMP_Text MonName;
    public Image nextEvoIcon;
    public List<Sprite> TypeCardBackgroundSprites;

    public void SetupItem(PokemonData pokemonData)
    {
        SetupType(pokemonData);
        MonImage.sprite = pokemonData.spritePokemonCard;
        MonName.text = pokemonData.PokemonName;
        nextEvoIcon.gameObject.SetActive(pokemonData.EvolutionPokemonData != null);
    }
    private void SetupType(PokemonData cardPokemon)
    {
        switch (cardPokemon.type)
        {
            case PokemonType.Fire:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[0];
                break;
            case PokemonType.Water:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[1];
                break;
            case PokemonType.Grass:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[2];
                break;
            case PokemonType.Electric:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[3];
                break;
            case PokemonType.Psychic:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[4];
                break;
            case PokemonType.Ice:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[5];
                break;
            case PokemonType.Dark:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[6];
                break;
            case PokemonType.Fighting:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[7];
                break;
            case PokemonType.Poison:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[8];
                break;
            case PokemonType.Ground:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[9];
                break;
        }
    }
}
