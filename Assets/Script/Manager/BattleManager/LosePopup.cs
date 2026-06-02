using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LosePopup : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public Button homeButton;

    [Header("SFX")]
    public AudioClip buttonClickSFX;
    public AudioClip loseSound;

    private const string HomeScene = "MainScene";
    private PopupScaleAnim _anim;
    public AudioSource audioSource;

    private void Awake()
    {
        _anim = GetComponent<PopupScaleAnim>();
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }

        panel?.SetActive(false);
        homeButton?.onClick.AddListener(() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX));
        homeButton?.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(HomeScene);
        });
    }

    public void Show(MatchTracker.LoseData data)
    {
        _anim ??= panel?.GetComponent<PopupScaleAnim>() ?? GetComponent<PopupScaleAnim>();
        Time.timeScale = 0f;
        panel?.SetActive(true);
        _anim.Open();

        if (loseSound != null && audioSource != null)
        {
            audioSource.clip = loseSound;
            audioSource.Play();
        }
    }
}
