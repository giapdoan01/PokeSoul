using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MapItemPrefab : MonoBehaviour
{
    public Image mapIconImage;
    public RectTransform mapIconRect;
    public GameObject doneIconObject;
    public Image doneIconImage;

    public Sprite spriteCompleted;
    public Sprite spriteNotCompleted;

    public TMP_Text mapNameText;
    public Button toBattleButton;

    [Header("Session Data (assign ScriptableObject asset)")]
    public BattleSessionData battleSessionData;

    private static readonly Color ColorUnlocked = Color.white;
    private static readonly Color ColorLocked    = new Color(0.35f, 0.35f, 0.35f, 1f);

    private const string BattleSceneName = "BattleScene";

    public void Setup(Map map, MapProgress progress, int index)
    {
        mapIconImage.sprite = map.mapSprite;
        mapNameText.text = map.mapName;

        bool isOdd = (index % 2) != 0;
        var pos = mapIconRect.anchoredPosition;
        pos.x = isOdd ? -240f : 240f;
        mapIconRect.anchoredPosition = pos;

        bool unlocked = progress != null;
        mapIconImage.color = unlocked ? ColorUnlocked : ColorLocked;

        if (!unlocked)
        {
            doneIconObject.SetActive(false);
            SetBattleButton(map, interactable: false);
            return;
        }

        doneIconObject.SetActive(true);
        doneIconImage.sprite = progress.isCompleted ? spriteCompleted : spriteNotCompleted;
        SetBattleButton(map, interactable: true);
    }

    private void SetBattleButton(Map map, bool interactable)
    {
        if (toBattleButton == null) return;

        toBattleButton.interactable = interactable;
        toBattleButton.onClick.RemoveAllListeners();

        if (interactable)
            toBattleButton.onClick.AddListener(() => StartBattle(map));
    }

    private void StartBattle(Map map)
    {
        if (battleSessionData != null)
            battleSessionData.selectedMap = map;

        SceneManager.LoadScene(BattleSceneName);
    }
}
