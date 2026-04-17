using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HomePageUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button openBattlePanel;
    public Button openShopPanel;
    public Button openCardPanel;

    [Header("Button Icons")]
    public RectTransform battleButtonIcon;
    public RectTransform shopButtonIcon;
    public RectTransform cardButtonIcon;

    // Thứ tự cố định: 0=Shop, 1=Battle, 2=Card
    [Header("Panels (thứ tự: Shop, Battle, Card)")]
    public RectTransform ShopPanel;
    public RectTransform BattlePanel;
    public RectTransform CardPanel;

    [Header("Animation")]
    public float pageWidth = 1500f;
    public float slideDuration = 0.35f;

    [Header("Icon Scale")]
    public float normalScale = 1f;
    public float selectedScale = 1.35f;
    public float scaleDuration = 0.15f;

    // index: 0=Shop, 1=Battle, 2=Card
    private int _currentIndex = 1; // Mặc định Battle
    private RectTransform[] _panels;
    private RectTransform[] _icons;

    void Start()
    {
        _panels = new RectTransform[] { ShopPanel, BattlePanel, CardPanel };
        _icons  = new RectTransform[] { shopButtonIcon, battleButtonIcon, cardButtonIcon };

        // Đặt vị trí ban đầu: Shop=-1500, Battle=0, Card=1500
        for (int i = 0; i < _panels.Length; i++)
            SetPanelX(_panels[i], (i - _currentIndex) * pageWidth);

        openShopPanel.onClick.AddListener(() => GoToPanel(0, shopButtonIcon));
        openBattlePanel.onClick.AddListener(() => GoToPanel(1, battleButtonIcon));
        openCardPanel.onClick.AddListener(() => GoToPanel(2, cardButtonIcon));

        // Icon ban đầu
        for (int i = 0; i < _icons.Length; i++)
            ScaleIconImmediate(_icons[i], i == _currentIndex ? selectedScale : normalScale);
    }

    private void GoToPanel(int targetIndex, RectTransform _)
    {
        if (targetIndex == _currentIndex) return;

        StopAllCoroutines();

        _currentIndex = targetIndex;

        // Slide tất cả panel cùng lúc theo delta
        for (int i = 0; i < _panels.Length; i++)
        {
            float fromX = _panels[i].anchoredPosition.x;
            float toX   = (i - targetIndex) * pageWidth;
            StartCoroutine(SlidePanel(_panels[i], fromX, toX));
        }

        // Scale icons
        for (int i = 0; i < _icons.Length; i++)
            StartCoroutine(ScaleIcon(_icons[i], i == targetIndex ? selectedScale : normalScale));
    }

    private IEnumerator SlidePanel(RectTransform panel, float fromX, float toX)
    {
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseInOutCubic(Mathf.Clamp01(elapsed / slideDuration));
            SetPanelX(panel, Mathf.Lerp(fromX, toX, t));
            yield return null;
        }
        SetPanelX(panel, toX);
    }

    private static void SetPanelX(RectTransform panel, float x)
    {
        var pos = panel.anchoredPosition;
        pos.x = x;
        panel.anchoredPosition = pos;
    }

    private static float EaseInOutCubic(float t)
        => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

    private IEnumerator ScaleIcon(RectTransform icon, float targetScale)
    {
        if (icon == null) yield break;
        Vector3 from = icon.localScale;
        Vector3 to   = Vector3.one * targetScale;
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / scaleDuration), 3f);
            icon.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
        icon.localScale = to;
    }

    private static void ScaleIconImmediate(RectTransform icon, float scale)
    {
        if (icon != null) icon.localScale = Vector3.one * scale;
    }
}
