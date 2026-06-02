using UnityEngine;
using System.Linq;

public class BattleSceneInitializer : MonoBehaviour
{
    [Header("Session Data (same ScriptableObject asset)")]
    public BattleSessionData battleSessionData;

    [Header("References")]
    public WaveManager waveManager;
    public CardDeckInBattleManager cardDeckInBattleManager;
    public PlayerStatsInBattleManager playerStatsInBattleManager;
    public MonUpgradePanel monUpgradePanel;

    private static readonly Vector3 MapSpawnPosition = new Vector3(-5f, 0f, -5f);

    private void Start()
    {
        if (battleSessionData == null || battleSessionData.selectedMap == null)
        {
            Debug.LogWarning("[BattleSceneInitializer] Không có map data trong session.");
            return;
        }

        Map map = battleSessionData.selectedMap;

        var bam = BattleAssetManager.Instance;
        GameObject mapPrefab = bam?.GetMapPrefab();
        if (mapPrefab == null)
        {
            Debug.LogError($"[BattleSceneInitializer] Map prefab chưa được load cho '{map.mapName}'!");
            return;
        }

        // Khởi động match tracker
        MatchTracker.Instance?.StartMatch(map);

        // Spawn map
        GameObject mapInstance = Instantiate(mapPrefab, MapSpawnPosition, Quaternion.identity);

        WayPointForEnemy wayPoint = mapInstance.GetComponentInChildren<WayPointForEnemy>();
        if (wayPoint == null)
            Debug.LogError($"[BattleSceneInitializer] mapPrefab '{map.mapName}' không có WayPointForEnemy!");

        // Inject dependencies vào tất cả PlacementSlot trong map
        foreach (var slot in mapInstance.GetComponentsInChildren<PlacementSlot>())
            slot.SetDependencies(playerStatsInBattleManager, monUpgradePanel);

        // Init wave
        if (waveManager != null)
        {
            if (playerStatsInBattleManager != null)
                waveManager.playerStatsInBattleManager = playerStatsInBattleManager;
            else
                Debug.LogWarning("[BattleSceneInitializer] playerStatsInBattleManager chưa được gán!");
            waveManager.Init(map, wayPoint);
            waveManager.StartBattle();
        }

        // Inject playerStats vào CardDeckInBattleManager
        if (cardDeckInBattleManager != null && playerStatsInBattleManager != null)
            cardDeckInBattleManager.playerStatsInBattleManager = playerStatsInBattleManager;

        // Setup deck từ PlayerData + preload Mon vào pool
        SetupDeck();
    }

    private void SetupDeck()
    {
        if (cardDeckInBattleManager == null) return;

        var pdm = PlayerDataManager.Instance;
        if (pdm == null || pdm.allPokemonData == null)
        {
            Debug.LogWarning("[BattleSceneInitializer] Không tìm thấy PlayerDataManager hoặc allPokemonData.");
            return;
        }

        PokemonData[] deck = pdm.GetBattleDeck()
            .Where(id => !string.IsNullOrEmpty(id))
            .Select(id => pdm.allPokemonData.GetPokemonDataById(id))
            .Where(p => p != null)
            .ToArray();

        // Preload Mon vào pool từ BattleAssetManager
        if (MonObjectPool.Instance != null)
            foreach (var mon in deck)
            {
                var prefab = BattleAssetManager.Instance?.GetMonPrefab(mon.id);
                if (prefab != null)
                    MonObjectPool.Instance.RegisterMon(mon.id, prefab);
            }

        cardDeckInBattleManager.SetupCardDeck(deck);
    }
}
