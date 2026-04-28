using System.Collections.Generic;
using UnityEngine;

public class SkillObjectPolling : MonoBehaviour
{
    public static SkillObjectPolling Instance { get; private set; }

    [SerializeField] private bool enableDebugLog;
    [SerializeField] private Transform pooledRoot;

    private readonly Dictionary<string, Queue<GameObject>> poolByKey = new Dictionary<string, Queue<GameObject>>();
    private readonly Dictionary<string, GameObject> prefabByKey = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (pooledRoot == null)
        {
            var root = new GameObject("SkillPool_Root");
            root.transform.SetParent(transform);
            pooledRoot = root.transform;
        }

        LogDebug("Skill pool manager ready.");
    }

    public void RegisterPrefab(string key, GameObject prefab, int preloadCount = 0)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.LogWarning("[SkillObjectPolling] Key is null or empty.");
            return;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"[SkillObjectPolling] Prefab of key '{key}' is null.");
            return;
        }

        prefabByKey[key] = prefab;
        LogDebug($"Register prefab. key={key}, prefab={prefab.name}, preload={preloadCount}");

        if (!poolByKey.ContainsKey(key))
        {
            poolByKey[key] = new Queue<GameObject>();
        }

        if (preloadCount <= 0)
        {
            return;
        }

        var queue = poolByKey[key];
        while (queue.Count < preloadCount)
        {
            var go = CreateNewInstance(key, prefab);
            ReturnToPoolInternal(key, go);
        }
    }

    public GameObject GetFromPool(string key, Vector3 position, Quaternion rotation)
    {
        if (!prefabByKey.ContainsKey(key))
        {
            Debug.LogWarning($"[SkillObjectPolling] Key '{key}' not registered.");
            return null;
        }

        if (!poolByKey.TryGetValue(key, out var queue))
        {
            queue = new Queue<GameObject>();
            poolByKey[key] = queue;
        }

        GameObject instance = null;

        while (queue.Count > 0 && instance == null)
        {
            instance = queue.Dequeue();
        }

        if (instance == null)
        {
            instance = CreateNewInstance(key, prefabByKey[key]);
        }

        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        LogDebug($"GetFromPool success. key={key}, object={instance.name}, queueCount={queue.Count}");

        return instance;
    }

    public void ReturnToPool(string key, GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        if (!poolByKey.ContainsKey(key))
        {
            poolByKey[key] = new Queue<GameObject>();
        }

        ReturnToPoolInternal(key, obj);
    }

    public void ReturnByInstance(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        var token = obj.GetComponent<SkillPoolToken>();
        if (token == null || string.IsNullOrWhiteSpace(token.PoolKey))
        {
            Destroy(obj);
            return;
        }

        ReturnToPool(token.PoolKey, obj);
    }

    private GameObject CreateNewInstance(string key, GameObject prefab)
    {
        var go = Instantiate(prefab, pooledRoot);
        go.name = $"{prefab.name}_Pooled";

        var token = go.GetComponent<SkillPoolToken>();
        if (token == null)
        {
            token = go.AddComponent<SkillPoolToken>();
        }

        token.PoolKey = key;
        return go;
    }

    private void ReturnToPoolInternal(string key, GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(pooledRoot);
        poolByKey[key].Enqueue(obj);
        LogDebug($"ReturnToPool. key={key}, object={obj.name}, queueCount={poolByKey[key].Count}");
    }

    private void LogDebug(string message)
    {
        if (!enableDebugLog)
        {
            return;
        }

        Debug.Log($"[SkillObjectPolling] {message}", this);
    }
}

public class SkillPoolToken : MonoBehaviour
{
    public string PoolKey { get; set; }

    public void ReturnToPool()
    {
        if (SkillObjectPolling.Instance == null)
        {
            Destroy(gameObject);
            return;
        }

        SkillObjectPolling.Instance.ReturnByInstance(gameObject);
    }

    public void ReturnToPoolAfterDelay(float delay)
    {
        StopAllCoroutines();
        StartCoroutine(ReturnAfterDelay(delay));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private System.Collections.IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }
}
