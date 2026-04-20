using UnityEngine;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
public class DeckSlot
{
    public string cardId;
    public DeckSlot() { cardId = ""; }
    public DeckSlot(string id) { cardId = id ?? ""; }
    public bool IsEmpty => string.IsNullOrEmpty(cardId);
}

[Serializable]
public class PlayerData
{
    public string username;
    public int level = 1;
    public List<string> ownCard = new();
    public List<DeckSlot> cardDeckToBattle = new();
    public int gem = 1000;

    public PlayerData() { InitDeck(); }

    public void InitDeck()
    {
        cardDeckToBattle = new List<DeckSlot>
        {
            new DeckSlot(), new DeckSlot(), new DeckSlot(), new DeckSlot()
        };
    }

    public void EnsureDeckIntegrity()
    {
        if (cardDeckToBattle == null)
            cardDeckToBattle = new List<DeckSlot>();

        while (cardDeckToBattle.Count < 4)
            cardDeckToBattle.Add(new DeckSlot());

        if (cardDeckToBattle.Count > 4)
            cardDeckToBattle = cardDeckToBattle.GetRange(0, 4);

        for (int i = 0; i < 4; i++)
            if (cardDeckToBattle[i] == null)
                cardDeckToBattle[i] = new DeckSlot();
    }

    public string GetCardIdAt(int position)
    {
        if (position < 0 || position >= 4) return null;
        var slot = cardDeckToBattle[position];
        return (slot == null || slot.IsEmpty) ? null : slot.cardId;
    }

    public void SetCardIdAt(int position, string cardId)
    {
        if (position < 0 || position >= 4) return;
        cardDeckToBattle[position] = new DeckSlot(cardId);
    }
    public List<string> GetOwnCard(){
        return ownCard;
    }
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }
    public PlayerData CurrentData { get; private set; }
    public AllPokemonData allPokemonData;
    public PokemonData[] myPokemonDatas;
    public Action<int> OnGemChanged;
    public Action OnPlayerDataLoaded;
    public Action OnOwnCardUpdated;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    async void OnApplicationQuit()
    {
        if (CurrentData != null)
            await SaveAsync();
    }

    public async Task LoadOrCreatePlayerDataAsync(string username)
    {
        try
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { "playerData" }
            );

            if (result.TryGetValue("playerData", out var item))
            {
                // ── Load raw JSON để tự xử lý migration ──
                string rawJson = item.Value.GetAsString();
                Debug.Log($"[LOAD] Raw JSON from cloud: {rawJson}");

                CurrentData = ParsePlayerDataWithMigration(rawJson);
                Debug.Log($"[LOAD] Loaded existing account: {CurrentData.username}");
                Debug.Log($"[LOAD] cardDeckToBattle: [{string.Join(", ", Enumerable.Range(0, 4).Select(i => $"{i}:{CurrentData.GetCardIdAt(i) ?? "null"}"))}]");

                // Nếu data vừa được migrate từ format cũ → save lại format mới
                if (_wasMigrated)
                {
                    Debug.Log("[LOAD] Data migrated from old format, saving new format...");
                    await SaveAsync();
                    _wasMigrated = false;
                }
            }
            else
            {
                // ── Tài khoản mới ──
                Debug.Log($"[LOAD] No data found → Creating new account: {username}");
                await CreateNewPlayerDataAsync(username);
            }

            RefreshMyPokemonDatas();
            OnGemChanged?.Invoke(CurrentData.gem);

            ShopManager shopManager = FindObjectOfType<ShopManager>();
            if (shopManager != null)
            {
                shopManager.SetupMyPokemonData(myPokemonDatas);
                shopManager.SetUpAllPokemonData(allPokemonData);
            }

            Debug.Log("[PlayerDataManager] OnPlayerDataLoaded event firing!");
            OnPlayerDataLoaded?.Invoke();
            Debug.Log("[PlayerDataManager] OnPlayerDataLoaded event fired!");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] LoadOrCreate error: {e.Message}\n{e.StackTrace}");
        }
    }

    private bool _wasMigrated = false;

    private PlayerData ParsePlayerDataWithMigration(string rawJson)
    {
        _wasMigrated = false;
        var jObj = JObject.Parse(rawJson);

        var data = new PlayerData();
        data.username = jObj["username"]?.ToString() ?? "";
        data.level    = jObj["level"]?.Value<int>() ?? 1;
        data.gem      = jObj["gem"]?.Value<int>() ?? 1000;
        data.ownCard  = jObj["ownCard"]?.ToObject<List<string>>() ?? new List<string>();

        // ── Detect & migrate cardDeckToBattle ──
        var deckToken = jObj["cardDeckToBattle"];
        if (deckToken == null || !deckToken.HasValues)
        {
            // Không có data → init 4 slot rỗng
            data.InitDeck();
            _wasMigrated = true;
            Debug.Log("[MIGRATE] No deck data found, initialized 4 empty slots.");
        }
        else
        {
            var firstElement = deckToken[0];

            if (firstElement?.Type == JTokenType.String)
            {
                // ── FORMAT CŨ: List<string> ──
                Debug.Log("[MIGRATE] Detected OLD format (List<string>), migrating...");
                var oldList = deckToken.ToObject<List<string>>() ?? new List<string>();
                _wasMigrated = true;

                // Handle bug 8 phần tử từ format cũ
                if (oldList.Count == 8)
                {
                    var first4  = oldList.GetRange(0, 4);
                    var second4 = oldList.GetRange(4, 4);
                    oldList = new List<string>();
                    for (int i = 0; i < 4; i++)
                        oldList.Add(!string.IsNullOrEmpty(first4[i]) ? first4[i] : second4[i]);
                    Debug.Log($"[MIGRATE] Fixed 8→4 elements: [{string.Join(", ", oldList.Select(x => x ?? "null"))}]");
                }

                // Convert string → DeckSlot
                data.cardDeckToBattle = new List<DeckSlot>();
                for (int i = 0; i < 4; i++)
                {
                    string cardId = (i < oldList.Count) ? oldList[i] : null;
                    data.cardDeckToBattle.Add(new DeckSlot(cardId));
                }
                Debug.Log($"[MIGRATE] Migration complete: [{string.Join(", ", Enumerable.Range(0, 4).Select(i => $"{i}:{data.GetCardIdAt(i) ?? "null"}"))}]");
            }
            else if (firstElement?.Type == JTokenType.Object)
            {
                // ── FORMAT MỚI: List<DeckSlot> ──
                Debug.Log("[LOAD] Detected NEW format (List<DeckSlot>), loading normally.");
                data.cardDeckToBattle = deckToken.ToObject<List<DeckSlot>>() ?? new List<DeckSlot>();
            }
            else
            {
                // Null token hoặc format không xác định → init an toàn
                Debug.LogWarning("[LOAD] Unknown deck format, initializing 4 empty slots.");
                data.InitDeck();
                _wasMigrated = true;
            }
        }

        data.EnsureDeckIntegrity();
        return data;
    }

    private async Task CreateNewPlayerDataAsync(string username)
    {
        CurrentData = new PlayerData
        {
            username = username,
            level    = 1,
            ownCard  = new List<string>(),
            gem      = 1000
        };
        CurrentData.InitDeck();
        await SaveAsync();
        Debug.Log($"[LOAD] Created new account: {username}");
    }

    private void RefreshMyPokemonDatas()
    {
        var pokemonList = new List<PokemonData>();
        if (allPokemonData != null)
        {
            foreach (var cardId in CurrentData.ownCard)
            {
                var pokemonData = allPokemonData.GetPokemonDataById(cardId);
                if (pokemonData != null)
                    pokemonList.Add(pokemonData);
            }
        }
        else
        {
            Debug.LogWarning("[PlayerDataManager] allPokemonData chưa được assign trong Inspector!");
        }
        myPokemonDatas = pokemonList.ToArray();
    }

    public async Task SaveAsync()
    {
        try
        {
            CurrentData.EnsureDeckIntegrity();
            Debug.Log($"[SAVE] cardDeckToBattle: [{string.Join(", ", Enumerable.Range(0, 4).Select(i => $"{i}:{CurrentData.GetCardIdAt(i) ?? "null"}"))}]");
            var data = new Dictionary<string, object> { { "playerData", CurrentData } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log("[SAVE] Save completed!");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] Save error: {e.Message}");
        }
    }

    public PokemonData GetPokemonDataByIdFromPlayerData(string id)
        => allPokemonData.GetPokemonDataById(id);

    public async Task AddCardToOwnCardAsync(string cardId)
    {
        if (!CurrentData.ownCard.Contains(cardId))
        {
            CurrentData.ownCard.Add(cardId);
            RefreshMyPokemonDatas();
            await SaveAsync();
            OnOwnCardUpdated?.Invoke();
            Debug.Log($"[PlayerDataManager] Added card {cardId} to ownCard");
        }
        else
        {
            Debug.LogWarning($"[PlayerDataManager] Card {cardId} already in ownCard");
        }
    }

    public async Task AddGemAsync(int amount)
    {
        CurrentData.gem += amount;
        OnGemChanged?.Invoke(CurrentData.gem);
        await SaveAsync();
        Debug.Log($"[PlayerDataManager] +{amount} gems → Total: {CurrentData.gem}");
    }

    public async Task MinusGemAsync(int amount)
    {
        if (CurrentData.gem >= amount)
        {
            CurrentData.gem -= amount;
            OnGemChanged?.Invoke(CurrentData.gem);
            await SaveAsync();
            Debug.Log($"[PlayerDataManager] -{amount} gems → Total: {CurrentData.gem}");
        }
        else
        {
            Debug.LogWarning($"[PlayerDataManager] Not enough gems. Current: {CurrentData.gem}, Required: {amount}");
        }
    }

    public async Task AddCardToBattleDeckAsync(string cardId, int position)
    {
        if (position < 0 || position >= 4)
        { Debug.LogError($"[PlayerDataManager] Invalid position: {position}"); return; }

        if (!string.IsNullOrEmpty(cardId) && !CurrentData.ownCard.Contains(cardId))
        { Debug.LogWarning($"[PlayerDataManager] Card {cardId} not in ownCard."); return; }

        CurrentData.SetCardIdAt(position, cardId);
        await SaveAsync();
        Debug.Log($"[PlayerDataManager] Set battle deck [{position}] = {cardId}");
    }

    public string GetCardFromBattleDeck(int position)
    {
        if (position < 0 || position >= 4)
        { Debug.LogError($"[PlayerDataManager] Invalid position: {position}"); return null; }
        return CurrentData.GetCardIdAt(position);
    }

    public async Task RemoveCardFromBattleDeckAsync(int position)
    {
        if (position < 0 || position >= 4)
        { Debug.LogError($"[PlayerDataManager] Invalid position: {position}"); return; }

        CurrentData.SetCardIdAt(position, null);
        await SaveAsync();
        Debug.Log($"[PlayerDataManager] Cleared battle deck slot [{position}]");
    }

    public List<string> GetBattleDeck()
        => Enumerable.Range(0, 4).Select(i => CurrentData.GetCardIdAt(i)).ToList();
}