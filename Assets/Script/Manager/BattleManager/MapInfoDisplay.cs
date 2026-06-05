using UnityEngine;
using TMPro;

// Gắn vào map prefab — hiển thị wave progress và enemy endpoint progress.
// Được inject bởi BattleSceneInitializer sau khi spawn map.
public class MapInfoDisplay : MonoBehaviour
{
    [Header("Wave Text — đặt tại cổng vào")]
    public TMP_Text waveText;

    [Header("Enemy Endpoint Text — đặt tại cổng ra")]
    public TMP_Text enemyEndpointText;

    public void UpdateWave(int currentWave, int totalWaves)
    {
        if (waveText != null)
            waveText.text = $"Wave {currentWave} / {totalWaves}";
    }

    public void UpdateEnemyEndpoint(int reached, int max)
    {
        if (enemyEndpointText != null)
            enemyEndpointText.text = $"{reached} / {max}";
    }
}
