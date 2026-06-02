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
        LoadingManager.Instance?.ShowLoading("Đang tải trận đấu...");

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
        {
            // Mon prefab
            var (monPrefab, monHandle) = await AddressableLoader.LoadAsync<GameObject>(pokemonData.pokemonPrefabRef);
            if (monPrefab != null)
                manager.StoreMonPrefab(pokemonData.id, monPrefab, monHandle);

            // Skill prefab
            var (skillPrefab, skillHandle) = await AddressableLoader.LoadAsync<GameObject>(pokemonData.skillPrefabRef);
            if (skillPrefab != null)
                manager.StoreSkillPrefab(pokemonData.id, skillPrefab, skillHandle);
        }

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

    private static BattleAssetManager EnsureBattleAssetManager()
    {
        if (BattleAssetManager.Instance != null)
            return BattleAssetManager.Instance;

        var go = new GameObject("BattleAssetManager");
        return go.AddComponent<BattleAssetManager>();
    }
}
