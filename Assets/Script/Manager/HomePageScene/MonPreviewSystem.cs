using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class MonPreviewSystem : MonoBehaviour
{
    public static MonPreviewSystem Instance { get; private set; }

    [Header("Spawn Points (index 0-3 = slot 1-4)")]
    public Transform[] spawnPoints = new Transform[4];

    private readonly GameObject[] _spawnedMons = new GameObject[4];
    private readonly List<AsyncOperationHandle> _handles = new();

    void Awake() => Instance = this;

    void Start()
    {
        var pdm = PlayerDataManager.Instance;
        if (pdm == null) { Debug.LogWarning("[MonPreviewSystem] PlayerDataManager is null."); return; }

        if (pdm.CurrentData != null)
            Refresh();
        else
            pdm.OnPlayerDataLoaded += Refresh;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        var pdm = PlayerDataManager.Instance;
        if (pdm != null) pdm.OnPlayerDataLoaded -= Refresh;
        ReleaseHandles();
    }

    public void Refresh()
    {
        RefreshAsync().Forget();
    }

    private async UniTaskVoid RefreshAsync()
    {
        var pdm = PlayerDataManager.Instance;
        if (pdm == null || pdm.CurrentData == null) return;

        // Destroy mons cũ + release handles cũ
        for (int i = 0; i < 4; i++)
        {
            if (_spawnedMons[i] != null) { Destroy(_spawnedMons[i]); _spawnedMons[i] = null; }
        }
        ReleaseHandles();

        for (int i = 0; i < 4; i++)
        {
            if (spawnPoints[i] == null) continue;

            string cardId = pdm.CurrentData.GetCardIdAt(i);
            if (string.IsNullOrEmpty(cardId)) continue;

            PokemonData data = pdm.allPokemonData?.GetPokemonDataById(cardId);
            if (data == null || !data.pokemonPrefabRef.RuntimeKeyIsValid()) continue;

            var (prefab, handle) = await AddressableLoader.LoadAsync<GameObject>(data.pokemonPrefabRef);
            if (prefab == null) continue;

            _handles.Add(handle);
            var mon = Instantiate(prefab, spawnPoints[i]);
            mon.transform.localPosition = Vector3.zero;
            mon.transform.localRotation = Quaternion.identity;
            _spawnedMons[i] = mon;
        }
    }

    private void ReleaseHandles()
    {
        AddressableLoader.ReleaseAll(_handles);
    }
}
