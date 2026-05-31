using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class WinPopup : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text mapNameText;
    public TMP_Text gemRewardText;
    public Button homeButton;

    [Header("SFX")]
    public AudioClip buttonClickSFX;

    [Header("Stars")]
    public Image star1;
    public Image star2;
    public Image star3;
    public Sprite spriteStar;
    public Sprite spriteNoStar;

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
            MatchTracker.Instance.OnWin += Show;
    }

    private void OnDisable()
    {
        if (MatchTracker.Instance != null)
            MatchTracker.Instance.OnWin -= Show;
    }

    private void Show(MatchTracker.WinData data)
    {
        if (mapNameText != null)   mapNameText.text   = data.mapName;
        if (gemRewardText != null) gemRewardText.text = $"+{data.gemReward}";
        SetStars(GetStarCount(data.enemiesReachedEnd));
        _anim.Open();
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
