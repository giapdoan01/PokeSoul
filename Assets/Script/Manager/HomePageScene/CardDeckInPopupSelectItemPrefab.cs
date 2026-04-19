using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CardDeckInPopupSelectItemPrefab : MonoBehaviour
{
    public Image frameCardWhenInBattleDeck;
    public Image backgroundCard;
    public Image monImage;
    public Image typeImage;
    public TMP_Text monName;
    public TMP_Text typeText;
    public Button selectCardButton;
    public List<Sprite> TypeSprites;
    public List<Sprite> typeCardBackgroundImageList;
    
    private PokemonData _pokemonData;
    private PopupListCardSelect _parentPopup;
    
    void Start()
    {
        selectCardButton.onClick.AddListener(OnSelectCardButtonClicked);
    }
    
    public void SetupCard(PokemonData pokemonData, PopupListCardSelect parentPopup, bool isInBattleDeck)
    {
        _pokemonData = pokemonData;
        _parentPopup = parentPopup;
        
        if (isInBattleDeck)
        {
            SetupCardInUsed(pokemonData);
        }
        else
        {
            frameCardWhenInBattleDeck.enabled = false;
            backgroundCard.enabled = true;
            monImage.sprite = pokemonData.spritePokemonCard;
            monName.text = pokemonData.PokemonName;
            typeText.text = pokemonData.type.ToString();
            selectCardButton.interactable = true;
            SetupType(pokemonData);
        }
    }
    
    public void SetupCardInUsed(PokemonData pokemonData)
    {
        frameCardWhenInBattleDeck.enabled = true;
        backgroundCard.enabled = true;
        monImage.sprite = pokemonData.spritePokemonCard;
        monName.text = pokemonData.PokemonName;
        typeText.text = pokemonData.type.ToString();
        selectCardButton.interactable = false;  // Không thể chọn thẻ đã được dùng
        SetupType(pokemonData);
    }
    
    private void OnSelectCardButtonClicked()
    {
        if (_parentPopup != null && _pokemonData != null)
        {
            _parentPopup.OnCardSelected(_pokemonData.id);
        }
    }
    
    private void SetupType(PokemonData cardPokemon)
    {
        switch (cardPokemon.type)
        {
            case PokemonType.Fire:
                typeImage.sprite = TypeSprites[0];
                backgroundCard.sprite = typeCardBackgroundImageList[0];
                break;
            case PokemonType.Water:
                typeImage.sprite = TypeSprites[1];
                backgroundCard.sprite = typeCardBackgroundImageList[1];
                break;
            case PokemonType.Grass:
                typeImage.sprite = TypeSprites[2];
                backgroundCard.sprite = typeCardBackgroundImageList[2];
                break;
            case PokemonType.Electric:
                typeImage.sprite = TypeSprites[3];
                backgroundCard.sprite = typeCardBackgroundImageList[3];
                break;
            case PokemonType.Psychic:
                typeImage.sprite = TypeSprites[4];
                backgroundCard.sprite = typeCardBackgroundImageList[4];
                break;
            case PokemonType.Ice:
                typeImage.sprite = TypeSprites[5];
                backgroundCard.sprite = typeCardBackgroundImageList[5];
                break;
            case PokemonType.Dark:
                typeImage.sprite = TypeSprites[6];
                backgroundCard.sprite = typeCardBackgroundImageList[6];
                break;
            case PokemonType.Fighting:
                typeImage.sprite = TypeSprites[7];
                backgroundCard.sprite = typeCardBackgroundImageList[7];
                break;
            case PokemonType.Poison:
                typeImage.sprite = TypeSprites[8];
                backgroundCard.sprite = typeCardBackgroundImageList[8];
                break;
            case PokemonType.Ground:
                typeImage.sprite = TypeSprites[9];
                backgroundCard.sprite = typeCardBackgroundImageList[9];
                break;
        }
    }
}