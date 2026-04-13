using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDeckItemPrefab : MonoBehaviour
{
    public GameObject CardParent;
    public Image pokemonCardImage;
    public TMP_Text pokemonNameText;
    public TMP_Text pokemonTypeText;
    public Image TypeImage;
    public List<Sprite> TypeSprites;
    public List<GameObject> cardTypeEffectPrefabs;

    public void Setup(PokemonData cardPokemon)
    {
        pokemonCardImage.sprite = cardPokemon.spritePokemonCard;
        pokemonNameText.text = cardPokemon.PokemonName;
        pokemonTypeText.text = cardPokemon.type.ToString();

    }
    public void setupCardEffect(int typeIndex = 0)
    {
        if (typeIndex < cardTypeEffectPrefabs.Count)
        {
            GameObject effectPrefab = cardTypeEffectPrefabs[typeIndex];
            GameObject effectInstance = Instantiate(effectPrefab, CardParent.transform);
            effectInstance.transform.SetSiblingIndex(0);
        }

    }
    public void SetupType(PokemonData cardPokemon)
    {
        switch (cardPokemon.type)
        {
            case PokemonType.Fire:
                TypeImage.sprite = TypeSprites[0];
                setupCardEffect(0);
                break;
            case PokemonType.Water:
                TypeImage.sprite = TypeSprites[1];
                setupCardEffect(1);
                break;
            case PokemonType.Grass:
                TypeImage.sprite = TypeSprites[2];
                setupCardEffect(2);
                break;
            case PokemonType.Electric:
                TypeImage.sprite = TypeSprites[3];
                setupCardEffect(3);
                break;
            case PokemonType.Psychic:
                TypeImage.sprite = TypeSprites[4];
                setupCardEffect(4);
                break;
            case PokemonType.Ice:
                TypeImage.sprite = TypeSprites[5];
                setupCardEffect(5);
                break;
            case PokemonType.Dark:
                TypeImage.sprite = TypeSprites[6];
                setupCardEffect(6);
                break;
            case PokemonType.Fighting:
                TypeImage.sprite = TypeSprites[7];
                setupCardEffect(7);
                break;
            case PokemonType.Poison:
                TypeImage.sprite = TypeSprites[8];
                setupCardEffect(8);
                break;
            case PokemonType.Ground:
                TypeImage.sprite = TypeSprites[9];
                setupCardEffect(9);
                break;
        }
    }
}