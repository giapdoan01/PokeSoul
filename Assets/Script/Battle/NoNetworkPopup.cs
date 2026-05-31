using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class NoNetworkPopup : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text messageText;
    public Button homeButton;

    [Header("SFX")]
    public AudioClip buttonClickSFX;

    private const string HomeScene = "MainScene";
    private PopupScaleAnim _anim;

    private void Awake()
    {
        _anim = GetComponent<PopupScaleAnim>();
        gameObject.SetActive(false);
        homeButton?.onClick.AddListener(() => SceneManager.LoadScene(HomeScene));
        homeButton?.onClick.AddListener(() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX));
    }

    private void OnEnable()
    {
        if (MatchTracker.Instance != null)
            MatchTracker.Instance.OnNetworkLost += Show;
    }

    private void OnDisable()
    {
        if (MatchTracker.Instance != null)
            MatchTracker.Instance.OnNetworkLost -= Show;
    }

    private void Show()
    {
        if (messageText != null)
            messageText.text = "Mất kết nối mạng!\nTrận đấu bị hủy.";
        _anim.Open();
    }
}
