using UnityEngine;
using System.Collections.Generic;

public class EnemyObjectPool : MonoBehaviour
{
    public static EnemyObjectPool Instance { get; private set; }

    [SerializeField] private Transform poolRoot;
    [SerializeField] private int preloadPerEnemy = 3;

    private readonly Dictionary<string, Queue<GameObject>> _pools = new();
    private readonly Dictionary<string, GameObject> _prefabs = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (poolRoot == null)
        {
            poolRoot = new GameObject("EnemyPool_Root").transform;
            poolRoot.SetParent(transform);
        }
    }

    public void RegisterEnemy(string key, GameObject prefab)
    {
        key = key?.Trim();
        if (string.IsNullOrEmpty(key) || prefab == null) return;
        _prefabs[key] = prefab;

        if (!_pools.ContainsKey(key))
            _pools[key] = new Queue<GameObject>();

        var queue = _pools[key];
        while (queue.Count < preloadPerEnemy)
            ReturnToPoolInternal(key, CreateInstance(key, prefab));
    }

    public GameObject Get(string key, Vector3 position, Quaternion rotation)
    {
        key = key?.Trim();
        if (!_prefabs.ContainsKey(key))
        {
            Debug.LogWarning($"[EnemyObjectPool] Key '{key}' chưa register — Instantiate trực tiếp.");
            return null;
        }

        if (!_pools.TryGetValue(key, out var queue) || queue.Count == 0)
        {
            var fresh = CreateInstance(key, _prefabs[key]);
            fresh.transform.SetPositionAndRotation(position, rotation);
            fresh.SetActive(true);
            return fresh;
        }

        var go = queue.Dequeue();
        go.transform.SetPositionAndRotation(position, rotation);
        go.SetActive(true);
        return go;
    }

    // Gọi khi muốn register + lấy ngay, an toàn dù gọi bất kỳ lúc nào
    public GameObject GetOrRegister(string key, GameObject prefab, Vector3 position, Quaternion rotation)
    {
        key = key?.Trim();
        if (!_prefabs.ContainsKey(key))
            RegisterEnemy(key, prefab);
        return Get(key, position, rotation);
    }

    public void Return(GameObject enemy)
    {
        if (enemy == null) return;
        var token = enemy.GetComponent<EnemyPoolToken>();
        if (token == null || string.IsNullOrEmpty(token.PoolKey))
        {
            Destroy(enemy);
            return;
        }
        ReturnToPoolInternal(token.PoolKey, enemy);
    }

    private void ReturnToPoolInternal(string key, GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(poolRoot);
        if (!_pools.ContainsKey(key)) _pools[key] = new Queue<GameObject>();
        _pools[key].Enqueue(go);
    }

    private GameObject CreateInstance(string key, GameObject prefab)
    {
        var go = Instantiate(prefab, poolRoot);
        go.name = $"{prefab.name}_Pooled";
        var token = go.GetComponent<EnemyPoolToken>() ?? go.AddComponent<EnemyPoolToken>();
        token.PoolKey = key;
        return go;
    }
}

public class EnemyPoolToken : MonoBehaviour
{
    public string PoolKey { get; set; }
}
