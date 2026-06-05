using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

// Gọi StartLoad() thay vì SceneManager.LoadScene trực tiếp.
// Load tất cả assets cần thiết trước khi vào BattleScene.
public class BattleSceneLoader : MonoBehaviour
{
    public static BattleSceneLoader Instance { get; private set; }

    private const string BattleSceneName = "BattleScene";
    private bool _isLoading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartLoad(Map map, BattleSessionData sessionData)
    {
        if (_isLoading)
        {
            Debug.LogWarning("[BattleSceneLoader] Already loading, ignoring duplicate call.");
            return;
        }
        // Chạy async trên main thread bằng cách bắt đầu coroutine wrapper
        RunLoadOnMainThread(map, sessionData);
    }

    // Dùng async void để Unity main thread chạy đúng — không dùng ContinueWith
    private async void RunLoadOnMainThread(Map map, BattleSessionData sessionData)
    {
        _isLoading = true;
        try
        {
            await LoadAndStartBattle(map, sessionData);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BattleSceneLoader] Unhandled exception during load: {ex}");
            LoadingManager.Instance?.HideLoading();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadAndStartBattle(Map map, BattleSessionData sessionData)
    {
        LoadingManager.Instance?.ShowLoading("Loading...");

        var manager = EnsureBattleAssetManager();
        manager.ReleaseAll();

        var pdm = PlayerDataManager.Instance;
        if (pdm == null || pdm.CurrentData == null)
        {
            Debug.LogError("[BattleSceneLoader] PlayerDataManager not ready.");
            LoadingManager.Instance?.HideLoading();
            return;
        }

        // ── Bước 1+2: Load Mon prefabs + Skill prefabs theo deck ──
        var deck = pdm.GetBattleDeck()
            .Where(id => !string.IsNullOrEmpty(id))
            .Select(id => pdm.allPokemonData?.GetPokemonDataById(id))
            .Where(data => data != null)
            .ToList();

        foreach (var pokemonData in deck)
        {
            bool ok = await LoadMonChainAsync(pokemonData, manager);
            if (!ok)
            {
                Debug.LogError($"[BattleSceneLoader] Failed to load Mon chain for: {pokemonData.id}. Aborting.");
                LoadingManager.Instance?.HideLoading();
                return;
            }
        }

        // ── Bước 3: Load Enemy prefabs từ spawnSequence (unique enemies) ──
        var loadedEnemyIds = new System.Collections.Generic.HashSet<string>();
        if (map.waves != null)
        {
            foreach (var wave in map.waves)
            {
                if (wave.spawnSequence == null) continue;
                foreach (var entry in wave.spawnSequence)
                {
                    if (entry.enemyData == null || !loadedEnemyIds.Add(entry.enemyData.id)) continue;

                    var (enemyPrefab, enemyHandle) = await AddressableLoader.LoadAsync<GameObject>(entry.enemyData.enemyPrefabRef);
                    if (enemyPrefab == null)
                    {
                        Debug.LogError($"[BattleSceneLoader] Failed to load enemy prefab: {entry.enemyData.id}. Aborting.");
                        LoadingManager.Instance?.HideLoading();
                        return;
                    }
                    manager.StoreEnemyPrefab(entry.enemyData.id, enemyPrefab, enemyHandle);
                }
            }
        }

        // ── Bước 4: Load Map prefab ──
        var (mapPrefab, mapHandle) = await AddressableLoader.LoadAsync<GameObject>(map.mapPrefabRef);
        if (mapPrefab == null)
        {
            Debug.LogError("[BattleSceneLoader] Failed to load map prefab. Aborting.");
            LoadingManager.Instance?.HideLoading();
            return;
        }
        manager.StoreMapPrefab(mapPrefab, mapHandle);

        // ── Bước 5: Lưu session và chuyển scene ──
        if (sessionData != null)
            sessionData.selectedMap = map;

        LoadingManager.Instance?.HideLoading();
        Debug.Log("[BattleSceneLoader] All assets loaded. Loading BattleScene...");
        SceneManager.LoadScene(BattleSceneName);
    }

    // Trả về true nếu load thành công, false nếu thất bại
    private static async Task<bool> LoadMonChainAsync(PokemonData data, BattleAssetManager manager)
    {
        if (data == null) return true;
        if (manager.HasMonPrefab(data.id)) return true;

        var (monPrefab, monHandle) = await AddressableLoader.LoadAsync<GameObject>(data.pokemonPrefabRef);
        if (monPrefab == null)
        {
            Debug.LogError($"[BattleSceneLoader] Failed to load mon prefab: {data.id}");
            return false;
        }
        manager.StoreMonPrefab(data.id, monPrefab, monHandle);

        var (skillPrefab, skillHandle) = await AddressableLoader.LoadAsync<GameObject>(data.skillPrefabRef);
        if (skillPrefab == null)
        {
            Debug.LogError($"[BattleSceneLoader] Failed to load skill prefab: {data.id}");
            return false;
        }
        manager.StoreSkillPrefab(data.id, skillPrefab, skillHandle);

        if (data.EvolutionPokemonData != null)
            return await LoadMonChainAsync(data.EvolutionPokemonData, manager);

        return true;
    }

    private static BattleAssetManager EnsureBattleAssetManager()
    {
        if (BattleAssetManager.Instance != null)
            return BattleAssetManager.Instance;

        var go = new GameObject("BattleAssetManager");
        return go.AddComponent<BattleAssetManager>();
    }
}
