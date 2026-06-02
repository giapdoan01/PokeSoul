using UnityEngine;
using System.Collections;

public class PlacementSlot : MonoBehaviour
{
    public bool IsOccupied { get; private set; }

    [Header("SFX")]
    public AudioClip spawnClip;
    public AudioSource audioSource;

    private GameObject _spawnedMon;
    private PlayerStatsInBattleManager _playerStats;
    private MonUpgradePanel _upgradePanel;

    private void OnEnable()  => PlacementSlotRegistry.Register(this);
    private void OnDisable() => PlacementSlotRegistry.Unregister(this);

    public void SetDependencies(PlayerStatsInBattleManager playerStats, MonUpgradePanel upgradePanel)
    {
        _playerStats = playerStats;
        _upgradePanel = upgradePanel;
    }

    public bool TrySpawn(PokemonData data)
    {
        if (IsOccupied || data == null) return false;

        var monPrefab = BattleAssetManager.Instance?.GetMonPrefab(data.id);
        if (monPrefab == null) return false;

        // Register nếu chưa có
        MonObjectPool.Instance?.RegisterMon(data.id, monPrefab);

        IsOccupied = true;
        _spawnedMon = MonObjectPool.Instance != null
            ? MonObjectPool.Instance.Get(data.id, transform.position, transform.rotation)
            : Instantiate(monPrefab, transform.position, transform.rotation);

        var monOnSlot = _spawnedMon.GetComponent<MonOnSlot>() ?? _spawnedMon.AddComponent<MonOnSlot>();
        monOnSlot.Init(data, this, _playerStats, _upgradePanel);

        StartCoroutine(SpawnPopAnim(_spawnedMon.transform));

        if (spawnClip != null && audioSource != null)
            audioSource.PlayOneShot(spawnClip);

        return true;
    }

    public void Evolve(PokemonData evoData)
    {
        ReturnCurrentMon();
        TrySpawn(evoData);
    }

    public void Clear()
    {
        ReturnCurrentMon();
    }

    private void ReturnCurrentMon()
    {
        if (_spawnedMon == null) return;
        if (MonObjectPool.Instance != null)
            MonObjectPool.Instance.Return(_spawnedMon);
        else
            Destroy(_spawnedMon);
        _spawnedMon = null;
        IsOccupied = false;
    }

    private static IEnumerator SpawnPopAnim(Transform t)
    {
        Vector3 originalScale = t.localScale;
        t.localScale = Vector3.zero;
        float elapsed = 0f;
        const float duration = 0.35f;
        const float c1 = 1.70158f, c3 = c1 + 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.Clamp01(elapsed / duration);
            float scale = 1f + c3 * Mathf.Pow(p - 1f, 3f) + c1 * Mathf.Pow(p - 1f, 2f);
            t.localScale = originalScale * Mathf.Max(0f, scale);
            yield return null;
        }

        t.localScale = originalScale;
    }
}
