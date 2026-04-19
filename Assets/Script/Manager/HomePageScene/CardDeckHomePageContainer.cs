using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class CardDeckHomePageContainer : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private GameObject cardDeckHomePageItemPrefab;
    [SerializeField] private int deckSlotCount = 4;

    private List<CardDeckHomePageItemPrefab> deckSlots = new();
    private bool isSlotsReady = false;
    private bool pendingLoadDeck = false;

    public static CardDeckHomePageContainer Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void OnEnable()
    {
        if (PlayerDataManager.Instance != null)
        {
            Debug.Log("[CardDeckHomePageContainer] Subscribing to OnPlayerDataLoaded event");
            PlayerDataManager.Instance.OnPlayerDataLoaded += OnPlayerDataLoaded;

            if (PlayerDataManager.Instance.CurrentData != null)
            {
                Debug.Log("[CardDeckHomePageContainer] Data already loaded, will load deck after slots ready");
                pendingLoadDeck = true;
            }
        }
        else
        {
            Debug.LogWarning("[CardDeckHomePageContainer] PlayerDataManager.Instance is NULL in OnEnable!");
        }
    }

    void OnDisable()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnPlayerDataLoaded -= OnPlayerDataLoaded;
        }
        isSlotsReady = false;
        pendingLoadDeck = false;
    }

    void Start()
    {
        SetupDeckSlots();
        isSlotsReady = true;

        if (pendingLoadDeck)
        {
            pendingLoadDeck = false;
            LoadCardDeckFromPlayerData();
        }
    }

    private void OnPlayerDataLoaded()
    {
        if (isSlotsReady)
            LoadCardDeckFromPlayerData();
        else
            pendingLoadDeck = true;
    }

    private void SetupDeckSlots()
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);
        deckSlots.Clear();

        for (int i = 0; i < deckSlotCount; i++)
        {
            GameObject slotObj = Instantiate(cardDeckHomePageItemPrefab, container);
            CardDeckHomePageItemPrefab slot = slotObj.GetComponent<CardDeckHomePageItemPrefab>();
            deckSlots.Add(slot);
            slot.SetupEmptyCard(i);
        }

        Debug.Log($"[CardDeckHomePageContainer] SetupDeckSlots complete. Slot count: {deckSlots.Count}");
    }

    private void LoadCardDeckFromPlayerData()
    {
        Debug.Log("[CardDeckHomePageContainer] LoadCardDeckFromPlayerData() called!");

        if (!isSlotsReady || deckSlots.Count == 0)
        {
            Debug.LogWarning("[CardDeckHomePageContainer] Slots not ready yet, marking as pending.");
            pendingLoadDeck = true;
            return;
        }

        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("[CardDeckHomePageContainer] PlayerDataManager.Instance is NULL!");
            return;
        }

        var playerData = PlayerDataManager.Instance.CurrentData;
        if (playerData == null)
        {
            Debug.LogError("[CardDeckHomePageContainer] PlayerData is NULL!");
            return;
        }

        Debug.Log($"[CardDeckHomePageContainer] cardDeckToBattle count: {playerData.cardDeckToBattle.Count}");

        for (int i = 0; i < deckSlotCount; i++)
        {
            // Reset slot về empty trước
            deckSlots[i].SetupEmptyCard(i);

            // Dùng GetCardIdAt() thay vì truy cập trực tiếp List<DeckSlot>
            string cardId = playerData.GetCardIdAt(i);
            Debug.Log($"[CardDeckHomePageContainer] Position {i}: cardId = {cardId ?? "null"}");

            if (!string.IsNullOrEmpty(cardId))
            {
                PokemonData pokemonData = PlayerDataManager.Instance.GetPokemonDataByIdFromPlayerData(cardId);
                Debug.Log($"[CardDeckHomePageContainer] Position {i}: pokemonData = {pokemonData?.PokemonName ?? "NULL"}");

                if (pokemonData != null)
                    deckSlots[i].SetupCard(pokemonData, i);
                else
                    Debug.LogWarning($"[CardDeckHomePageContainer] PokemonData not found for cardId: {cardId}");
            }
            else
            {
                Debug.Log($"[CardDeckHomePageContainer] Position {i}: empty slot");
            }
        }

        Debug.Log("[CardDeckHomePageContainer] Loaded battle deck complete.");
    }

    public void RefreshDeckUI()
    {
        LoadCardDeckFromPlayerData();
        Debug.Log("Battle deck UI refreshed");
    }
}