using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CardDeckInBattleManager : MonoBehaviour
{
    public GameObject CardInBattleItemPrefabObject;
    public Transform cardListContainer;

    [Header("Ghost Image (kéo Image từ scene vào đây)")]
    public Image dragGhostImage;

    [Header("Player Stats")]
    public PlayerStatsInBattleManager playerStatsInBattleManager;

    private PokemonData[] _playerDeck;

    public void SetupCardDeck(PokemonData[] deck)
    {
        if (cardListContainer == null)
        {
            Debug.LogError("[CardDeckInBattleManager] cardListContainer chưa được gán trong Inspector!");
            return;
        }
        if (CardInBattleItemPrefabObject == null)
        {
            Debug.LogError("[CardDeckInBattleManager] CardInBattleItemPrefabObject chưa được gán!");
            return;
        }

        _playerDeck = deck;

        for (int i = cardListContainer.childCount - 1; i >= 0; i--)
            Destroy(cardListContainer.GetChild(i).gameObject);

        foreach (var cardData in _playerDeck)
        {
            if (cardData == null) continue;
            var go = Instantiate(CardInBattleItemPrefabObject, cardListContainer);
            var item = go.GetComponent<CardInBattleItemPrefab>();
            if (item == null) continue;
            item.SetCardData(cardData);
            item.SetDragGhost(dragGhostImage);
            item.SetPlayerStats(playerStatsInBattleManager);
        }
    }
}
