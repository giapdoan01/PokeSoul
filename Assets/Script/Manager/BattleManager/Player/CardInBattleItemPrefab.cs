using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CardInBattleItemPrefab : MonoBehaviour
{
    public PokemonData cardData;
    public Image cardImage;
    public Text cardNameText;
    public Text cardCostText;
    public Image TypeImage;
    public Sprite[] TypeSprites;
    public void SetCardData(PokemonData data)
    {
        cardData = data;
        cardImage.sprite = cardData.spritePokemonCard;
        cardNameText.text = cardData.PokemonName;
        cardCostText.text = cardData.getPokemonLevelDataByLevel(1).GetStatEntryByName("Cost").value.ToString();
        SetUpTypeSprite(cardData);
    }
    public void SetUpTypeSprite(PokemonData data)
    {
        switch (data.type)
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
            case PokemonType.Dark:
                TypeImage.sprite = TypeSprites[6];
                break;
            case PokemonType.Fighting:
                TypeImage.sprite = TypeSprites[7];
                break;
            case PokemonType.Poison:
                TypeImage.sprite = TypeSprites[8];
                break;
            case PokemonType.Ground:
                TypeImage.sprite = TypeSprites[9];
                break;
        }
    }

}