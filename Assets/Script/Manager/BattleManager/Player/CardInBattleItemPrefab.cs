using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

public class CardInBattleItemPrefab : MonoBehaviour
{
    public PokemonData cardData;
    public Image cardImage;
    public Image monImage;
    public TMP_Text cardNameText;
    public TMP_Text typeText;
    public TMP_Text cardCostText;
    public Image TypeImage;
    public Sprite[] TypeSprites;
    public Sprite[] TypeBackgroundSprites;
    public void SetCardData(PokemonData data)
    {
        cardData = data;
        monImage.sprite = cardData.spritePokemonCard;
        cardNameText.text = cardData.PokemonName;
        typeText.text = cardData.type.ToString();
        cardCostText.text = cardData.getPokemonLevelDataByLevel(1).GetStatEntryByName("Cost").value.ToString();
        SetUpTypeSprite(cardData);
    }
    public void SetUpTypeSprite(PokemonData data)
    {
        switch (data.type)
        {
            case PokemonType.Fire:
                TypeImage.sprite = TypeSprites[0];
                cardImage.sprite = TypeBackgroundSprites[0];
                break;
            case PokemonType.Water:
                TypeImage.sprite = TypeSprites[1];
                cardImage.sprite = TypeBackgroundSprites[1];
                break;
            case PokemonType.Grass:
                TypeImage.sprite = TypeSprites[2];
                cardImage.sprite = TypeBackgroundSprites[2];
                break;
            case PokemonType.Electric:
                TypeImage.sprite = TypeSprites[3];
                cardImage.sprite = TypeBackgroundSprites[3];
                break;
            case PokemonType.Psychic:
                TypeImage.sprite = TypeSprites[4];
                cardImage.sprite = TypeBackgroundSprites[4];
                break;
            case PokemonType.Ice:
                TypeImage.sprite = TypeSprites[5];
                cardImage.sprite = TypeBackgroundSprites[5];
                break;
            case PokemonType.Dark:
                TypeImage.sprite = TypeSprites[6];
                cardImage.sprite = TypeBackgroundSprites[6];
                break;
            case PokemonType.Fighting:
                TypeImage.sprite = TypeSprites[7];
                cardImage.sprite = TypeBackgroundSprites[7];
                break;
            case PokemonType.Poison:
                TypeImage.sprite = TypeSprites[8];
                cardImage.sprite = TypeBackgroundSprites[8];
                break;
            case PokemonType.Ground:
                TypeImage.sprite = TypeSprites[9];
                cardImage.sprite = TypeBackgroundSprites[9];
                break;
        }
    }

}