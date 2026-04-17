using UnityEngine;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

[Serializable]
public class PlayerData
{
    public string username;
    public int level = 1;
    public List<string> ownCard = new();
    public List<string> cardDeckToBattle = new();
    public int gem = 1000;
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }
    public PlayerData CurrentData { get; private set; }
    public AllPokemonData allPokemonData;
    public PokemonData[] myPokemonDatas;
    public Action<int> OnGemChanged;

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

    // Gọi sau khi login/register thành công
    public async Task LoadOrCreatePlayerDataAsync(string username)
    {
        try
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "playerData" });

            if (result.TryGetValue("playerData", out var item))
            {
                CurrentData = item.Value.GetAs<PlayerData>();
                Debug.Log($"Loaded data for: {CurrentData.username}");
            }
            else
            {
                await CreateNewPlayerDataAsync(username);
            }
            // Load PokemonData cho player (ownCard rỗng khi mới tạo acc là bình thường)
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
            OnGemChanged?.Invoke(CurrentData.gem);
            // Setup Shop
            ShopManager shopManager = FindObjectOfType<ShopManager>();
            if (shopManager != null)
            {
                shopManager.SetupMyPokemonData(myPokemonDatas);
                shopManager.SetUpAllPokemonData(allPokemonData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"LoadOrCreate error: {e.Message}");
        }
    }

    private async Task CreateNewPlayerDataAsync(string username)
    {
        CurrentData = new PlayerData
        {
            username = username,
            level = 1,
            ownCard = new List<string>(),
            cardDeckToBattle = new List<string>(),
            gem = 1000
        };

        await SaveAsync();
        Debug.Log($"Created new data for: {username}");
    }

    public async Task SaveAsync()
    {
        try
        {
            var data = new Dictionary<string, object> { { "playerData", CurrentData } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }
        catch (Exception e)
        {
            Debug.LogError($"Save error: {e.Message}");
        }
    }
    public PokemonData GetPokemonDataByIdFromPlayerData(string id)
    {
        return allPokemonData.GetPokemonDataById(id);
    }
    public async Task AddCardToOwnCardAsync(string cardId)
    {
        if (!CurrentData.ownCard.Contains(cardId))
        {
            CurrentData.ownCard.Add(cardId);
            await SaveAsync();
            Debug.Log($"Added card {cardId} to ownCard");
        }
        else
        {
            Debug.LogWarning($"Card {cardId} already exists in ownCard");
        }
    }
    public async Task AddGemAsync(int amount)
    {
        CurrentData.gem += amount;
        OnGemChanged?.Invoke(CurrentData.gem);
        await SaveAsync();
        Debug.Log($"Added {amount} gems. Total now: {CurrentData.gem}");
    }
    public async Task MinusGemAsync(int amount)
    {
        if (CurrentData.gem >= amount)
        {
            CurrentData.gem -= amount;
            OnGemChanged?.Invoke(CurrentData.gem);
            await SaveAsync();
            Debug.Log($"Subtracted {amount} gems. Total now: {CurrentData.gem}");
        }
        else
        {
            Debug.LogWarning($"Not enough gems to subtract. Total now: {CurrentData.gem}");
        }
    }
}
