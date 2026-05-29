using UnityEngine;

public class MonPreviewSystem : MonoBehaviour
{
    [Header("Spawn Points (index 0-3 = slot 1-4)")]
    public Transform[] spawnPoints = new Transform[4];

    private GameObject[] _spawnedMons = new GameObject[4];

    void Start()
    {
        var pdm = PlayerDataManager.Instance;
        if (pdm == null)
        {
            Debug.LogWarning("[MonPreviewSystem] PlayerDataManager.Instance is null.");
            return;
        }

        if (pdm.CurrentData != null)
            Refresh();
        else
            pdm.OnPlayerDataLoaded += Refresh;
    }

    void OnDestroy()
    {
        var pdm = PlayerDataManager.Instance;
        if (pdm != null)
            pdm.OnPlayerDataLoaded -= Refresh;
    }

    public void Refresh()
    {
        var pdm = PlayerDataManager.Instance;
        if (pdm == null || pdm.CurrentData == null) return;

        for (int i = 0; i < 4; i++)
        {
            if (_spawnedMons[i] != null)
            {
                Destroy(_spawnedMons[i]);
                _spawnedMons[i] = null;
            }

            if (spawnPoints[i] == null) continue;

            string cardId = pdm.CurrentData.GetCardIdAt(i);
            if (string.IsNullOrEmpty(cardId)) continue;

            PokemonData data = pdm.allPokemonData?.GetPokemonDataById(cardId);
            if (data == null || data.pokemonPrefab == null) continue;

            GameObject mon = Instantiate(data.pokemonPrefab, spawnPoints[i]);
            mon.transform.localPosition = Vector3.zero;
            mon.transform.localRotation = Quaternion.identity;
            _spawnedMons[i] = mon;
        }
    }
}
