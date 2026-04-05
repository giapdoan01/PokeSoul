using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDeckItemPrefab : MonoBehaviour
{
    public Image pokemonCardImage;
    public Text pokemonNameText;
    public Text pokemonTypeText;
    public Image TypeImage;
    public List<Sprite> TypeSprites;

    public void Setup(CardPokemonSO cardPokemon)
    {
        pokemonCardImage.sprite = cardPokemon.spritePokemonCard;
        pokemonNameText.text = cardPokemon.pokemonName;
        pokemonTypeText.text = cardPokemon.type.ToString();

    }
    public void SetupType(CardPokemonSO cardPokemon)
    {
        switch (cardPokemon.type)
        {
            case PokemonType.Fire:
                TypeImage.sprite = TypeSprites[0];
                break;
            case PokemonType.Water:
                TypeImage.sprite = TypeSprites[1];
                break;
            case PokemonType.Grass:
                TypeImage.sprite = TypeSprites[2];
                break;
            case PokemonType.Electric:
                TypeImage.sprite = TypeSprites[3];
                break;
            case PokemonType.Psychic:
                TypeImage.sprite = TypeSprites[4];
                break;
            case PokemonType.Ice:
                TypeImage.sprite = TypeSprites[5];
                break;
            case PokemonType.Dragon:
                TypeImage.sprite = TypeSprites[6];
                break;
            case PokemonType.Dark:
                TypeImage.sprite = TypeSprites[7];
                break;
            case PokemonType.Fighting:
                TypeImage.sprite = TypeSprites[8];
                break;
            case PokemonType.Normal:
                TypeImage.sprite = TypeSprites[9];
                break;
        }
    }
}