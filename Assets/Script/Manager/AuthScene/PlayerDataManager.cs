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
}
