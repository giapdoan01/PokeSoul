using UnityEngine;
using UnityEngine.UI;

public class MapItemPrefab : MonoBehaviour
{
    public Image mapIconImage;
    public RectTransform mapIconRect;
    public GameObject doneIconObject;
    public Image doneIconImage;

    public Sprite spriteCompleted;
    public Sprite spriteNotCompleted;

    private static readonly Color ColorUnlocked = Color.white;
    private static readonly Color ColorLocked    = new Color(0.35f, 0.35f, 0.35f, 1f);

    public void Setup(Map map, MapProgress progress, int index)
    {
        // Sprite
        mapIconImage.sprite = map.mapSprite;

        // posX: id lẻ = -240, id chẵn = 240
        bool isOdd = (index % 2) != 0;
        var pos = mapIconRect.anchoredPosition;
        pos.x = isOdd ? -240f : 240f;
        mapIconRect.anchoredPosition = pos;

        // Locked / Unlocked
        bool unlocked = progress != null;
        mapIconImage.color = unlocked ? ColorUnlocked : ColorLocked;

        if (!unlocked)
        {
            doneIconObject.SetActive(false);
            return;
        }

        doneIconObject.SetActive(true);
        doneIconImage.sprite = progress.isCompleted ? spriteCompleted : spriteNotCompleted;
    }
}
