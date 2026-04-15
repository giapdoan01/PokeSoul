using UnityEngine;

public class CardDeckInBattleManager : MonoBehaviour
{
    public GameObject CardInBattleItemPrefabObject;
    public Transform cardListContainer;
    public PokemonData[] playerDeck;
    void Start()
    {
        SetupCardDeck(playerDeck);
    }
    public void SetupCardDeck(PokemonData[] deck)
    {
        playerDeck = deck;
        foreach (var cardData in playerDeck)
        {
            GameObject cardItemObj = Instantiate(CardInBattleItemPrefabObject, cardListContainer);
            CardInBattleItemPrefab cardItem = cardItemObj.GetComponent<CardInBattleItemPrefab>();
            cardItem.SetCardData(cardData);
        }
    }
    public void SetupDataPlayerDeck(PokemonData[] deck)
    {
        playerDeck = deck;
    }
}
