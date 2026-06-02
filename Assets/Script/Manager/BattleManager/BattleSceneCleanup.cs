using UnityEngine;

// Gắn vào bất kỳ GameObject nào trong BattleScene.
// Tự động release toàn bộ Addressable handles khi thoát BattleScene.
public class BattleSceneCleanup : MonoBehaviour
{
    private void OnDestroy()
    {
        BattleAssetManager.Instance?.ReleaseAll();
        Debug.Log("[BattleSceneCleanup] Battle assets released.");
    }
}
