using UnityEngine;
using System.Collections.Generic;

public class MonObjectPool : MonoBehaviour
{
    public static MonObjectPool Instance { get; private set; }

    [SerializeField] private Transform poolRoot;
    [SerializeField] private int preloadPerMon = 2;

    private readonly Dictionary<string, Queue<GameObject>> _pools = new();
    private readonly Dictionary<string, GameObject> _prefabs = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (poolRoot == null)
        {
            poolRoot = new GameObject("MonPool_Root").transform;
            poolRoot.SetParent(transform);
        }
    }

    public void RegisterMon(string key, GameObject prefab)
    {
        if (string.IsNullOrEmpty(key) || prefab == null) return;
        _prefabs[key] = prefab;

        if (!_pools.ContainsKey(key))
            _pools[key] = new Queue<GameObject>();

        var queue = _pools[key];
        while (queue.Count < preloadPerMon)
            ReturnInternal(key, CreateInstance(key, prefab));
    }

    public GameObject Get(string key, Vector3 position, Quaternion rotation)
    {
        if (!_prefabs.ContainsKey(key))
        {
            Debug.LogWarning($"[MonObjectPool] Key '{key}' chưa được register!");
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

    public void Return(GameObject mon)
    {
        if (mon == null) return;
        var token = mon.GetComponent<MonPoolToken>();
        if (token == null || string.IsNullOrEmpty(token.PoolKey))
        {
            Destroy(mon);
            return;
        }
        ReturnInternal(token.PoolKey, mon);
    }

    private void ReturnInternal(string key, GameObject go)
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
        var token = go.GetComponent<MonPoolToken>() ?? go.AddComponent<MonPoolToken>();
        token.PoolKey = key;
        return go;
    }
}

public class MonPoolToken : MonoBehaviour
{
    public string PoolKey { get; set; }
}
