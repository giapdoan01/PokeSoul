using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsInBattleUI : MonoBehaviour
{
    public TMP_Text coinText;
    public PlayerStatsInBattleManager playerStatsInBattleManager;
    public Button settingsButton;
    public SettingsPopup settingsPopup;

    void Awake()
    {
        if (playerStatsInBattleManager == null)
            Debug.LogError("[PlayerStatsInBattleUI] Không tìm thấy PlayerStatsInBattleManager trong scene!");

        playerStatsInBattleManager.OnCoinChanged += UpdateCoinText;
        settingsButton?.onClick.AddListener(() => settingsPopup?.Open());
    }
    void Start()
    {
        if (playerStatsInBattleManager != null)
            UpdateCoinText(playerStatsInBattleManager.playerCoin);
    }
    void UpdateCoinText(int coin)
    {
        if (coinText != null)
        {
            coinText.text = $"{coin}";
        }
    }

}