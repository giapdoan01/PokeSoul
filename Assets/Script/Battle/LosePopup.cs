using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LosePopup : MonoBehaviour
{
    [Header("UI")]
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
            MatchTracker.Instance.OnLose += Show;
    }

    private void OnDisable()
    {
        if (MatchTracker.Instance != null)
            MatchTracker.Instance.OnLose -= Show;
    }

    private void Show(MatchTracker.LoseData data)
    {
        _anim.Open();
    }
}
