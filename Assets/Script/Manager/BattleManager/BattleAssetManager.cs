using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

// Singleton lưu trữ tất cả prefabs đã load cho 1 trận đấu.
// Được populate bởi BattleSceneLoader trước khi vào BattleScene.
// Gọi ReleaseAll() khi thoát BattleScene.
public class BattleAssetManager : MonoBehaviour
{
    public static BattleAssetManager Instance { get; private set; }

    private readonly Dictionary<string, GameObject> _monPrefabs    = new();
    private readonly Dictionary<string, GameObject> _skillPrefabs  = new();
    private readonly Dictionary<string, GameObject> _enemyPrefabs  = new();
    private readonly List<AsyncOperationHandle>     _handles        = new();
    private GameObject _mapPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── Store (gọi từ BattleSceneLoader) ──

    public void StoreMonPrefab(string pokemonId, GameObject prefab, AsyncOperationHandle handle)
    {
        _monPrefabs[pokemonId] = prefab;
        _handles.Add(handle);
    }

    public void StoreSkillPrefab(string pokemonId, GameObject prefab, AsyncOperationHandle handle)
    {
        _skillPrefabs[pokemonId] = prefab;
        _handles.Add(handle);
    }

    public void StoreEnemyPrefab(string enemyId, GameObject prefab, AsyncOperationHandle handle)
    {
        _enemyPrefabs[enemyId] = prefab;
        _handles.Add(handle);
    }

    public void StoreMapPrefab(GameObject prefab, AsyncOperationHandle handle)
    {
        _mapPrefab = prefab;
        _handles.Add(handle);
    }

    // ── Getter (sync — dùng trong BattleScene) ──

    public bool HasMonPrefab(string pokemonId) => _monPrefabs.ContainsKey(pokemonId);

    public GameObject GetMonPrefab(string pokemonId)
    {
        if (_monPrefabs.TryGetValue(pokemonId, out var prefab)) return prefab;
        Debug.LogError($"[BattleAssetManager] Mon prefab not found: {pokemonId}");
        return null;
    }

    public GameObject GetSkillPrefab(string pokemonId)
    {
        if (_skillPrefabs.TryGetValue(pokemonId, out var prefab)) return prefab;
        Debug.LogError($"[BattleAssetManager] Skill prefab not found: {pokemonId}");
        return null;
    }

    public GameObject GetEnemyPrefab(string enemyId)
    {
        if (_enemyPrefabs.TryGetValue(enemyId, out var prefab)) return prefab;
        Debug.LogError($"[BattleAssetManager] Enemy prefab not found: {enemyId}");
        return null;
    }

    public GameObject GetMapPrefab()
    {
        if (_mapPrefab != null) return _mapPrefab;
        Debug.LogError("[BattleAssetManager] Map prefab not found.");
        return null;
    }

    // ── Cleanup (gọi khi thoát BattleScene) ──

    public void ReleaseAll()
    {
        AddressableLoader.ReleaseAll(_handles);
        _monPrefabs.Clear();
        _skillPrefabs.Clear();
        _enemyPrefabs.Clear();
        _mapPrefab = null;
        Debug.Log("[BattleAssetManager] All battle assets released.");
    }
}
