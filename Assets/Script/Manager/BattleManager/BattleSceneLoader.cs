using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

// Gọi LoadAndStartBattle() thay vì SceneManager.LoadScene trực tiếp.
// Load tất cả assets cần thiết trước khi vào BattleScene.
public class BattleSceneLoader : MonoBehaviour
{
    public static BattleSceneLoader Instance { get; private set; }

    private const string BattleSceneName = "BattleScene";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartLoad(Map map, BattleSessionData sessionData)
    {
        LoadAndStartBattle(map, sessionData).ContinueWith(t =>
        {
            if (t.IsFaulted)
                Debug.LogError($"[BattleSceneLoader] Load failed: {t.Exception}");
        });
    }

    private async Task LoadAndStartBattle(Map map, BattleSessionData sessionData)
    {
        LoadingManager.Instance?.ShowLoading("Loading...");

        var manager = EnsureBattleAssetManager();
        manager.ReleaseAll(); // clear trận cũ nếu có

        var pdm = PlayerDataManager.Instance;
        if (pdm == null || pdm.CurrentData == null)
        {
            Debug.LogError("[BattleSceneLoader] PlayerDataManager not ready.");
            LoadingManager.Instance?.HideLoading();
            return;
        }

        // ── Bước 1+2: Load 4 Mon prefabs + Skill prefabs theo deck ──
        var deck = pdm.GetBattleDeck()
            .Where(id => !string.IsNullOrEmpty(id))
            .Select(id => pdm.allPokemonData?.GetPokemonDataById(id))
            .Where(data => data != null)
            .ToList();

        foreach (var pokemonData in deck)
            await LoadMonChainAsync(pokemonData, manager);

        // ── Bước 3: Load Enemy prefabs theo map ──
        if (map.enemyDatas != null)
        {
            foreach (var enemyData in map.enemyDatas)
            {
                var (enemyPrefab, enemyHandle) = await AddressableLoader.LoadAsync<GameObject>(enemyData.enemyPrefabRef);
                if (enemyPrefab != null)
                    manager.StoreEnemyPrefab(enemyData.id, enemyPrefab, enemyHandle);
            }
        }

        // ── Bước 4: Load Map prefab ──
        var (mapPrefab, mapHandle) = await AddressableLoader.LoadAsync<GameObject>(map.mapPrefabRef);
        if (mapPrefab != null)
            manager.StoreMapPrefab(mapPrefab, mapHandle);

        // ── Bước 5: Lưu session và chuyển scene ──
        if (sessionData != null)
            sessionData.selectedMap = map;

        LoadingManager.Instance?.HideLoading();
        Debug.Log("[BattleSceneLoader] All assets loaded. Loading BattleScene...");
        SceneManager.LoadScene(BattleSceneName);
    }

    // Load prefab + skill của 1 Mon và toàn bộ evolution chain của nó
    private static async Task LoadMonChainAsync(PokemonData data, BattleAssetManager manager)
    {
        if (data == null) return;

        // Bỏ qua nếu đã load rồi (tránh load trùng khi 2 Mon cùng evo chain)
        if (manager.HasMonPrefab(data.id)) return;

        var (monPrefab, monHandle) = await AddressableLoader.LoadAsync<GameObject>(data.pokemonPrefabRef);
        if (monPrefab != null)
            manager.StoreMonPrefab(data.id, monPrefab, monHandle);

        var (skillPrefab, skillHandle) = await AddressableLoader.LoadAsync<GameObject>(data.skillPrefabRef);
        if (skillPrefab != null)
            manager.StoreSkillPrefab(data.id, skillPrefab, skillHandle);

        // Load tiếp evolution nếu có
        if (data.EvolutionPokemonData != null)
            await LoadMonChainAsync(data.EvolutionPokemonData, manager);
    }

    private static BattleAssetManager EnsureBattleAssetManager()
    {
        if (BattleAssetManager.Instance != null)
            return BattleAssetManager.Instance;

        var go = new GameObject("BattleAssetManager");
        return go.AddComponent<BattleAssetManager>();
    }
}
