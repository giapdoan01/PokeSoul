using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class WinPopup : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public TMP_Text mapNameText;
    public TMP_Text gemRewardText;
    public Button homeButton;

    [Header("SFX")]
    public AudioClip buttonClickSFX;
    public AudioClip winSound;

    [Header("Stars")]
    public Image star1;
    public Image star2;
    public Image star3;
    public Sprite spriteStar;
    public Sprite spriteNoStar;

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

    public void Show(MatchTracker.WinData data)
    {
        _anim ??= panel?.GetComponent<PopupScaleAnim>() ?? GetComponent<PopupScaleAnim>();
        Time.timeScale = 0f;
        panel?.SetActive(true);
        if (mapNameText != null)   mapNameText.text   = data.mapName;
        if (gemRewardText != null) gemRewardText.text = $"+{data.gemReward}";
        SetStars(GetStarCount(data.enemiesReachedEnd));
        _anim.Open();

        if (winSound != null && audioSource != null)
        {
            audioSource.clip = winSound;
            audioSource.Play();
        }
    }

    private static int GetStarCount(int enemiesReached)
    {
        if (enemiesReached < 3)  return 3;
        if (enemiesReached <= 5) return 2;
        return 1;
    }

    private void SetStars(int count)
    {
        if (star1 != null) star1.sprite = count >= 1 ? spriteStar : spriteNoStar;
        if (star2 != null) star2.sprite = count >= 2 ? spriteStar : spriteNoStar;
        if (star3 != null) star3.sprite = count >= 3 ? spriteStar : spriteNoStar;
    }
}
