using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class HomePageUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button openBattlePanel;
    public Button openShopPanel;
    public Button openCardPanel;
    public Button openListMapButton;

    [Header("Button Icons")]
    public RectTransform battleButtonIcon;
    public RectTransform shopButtonIcon;
    public RectTransform cardButtonIcon;

    // Thứ tự cố định: 0=Shop, 1=Battle, 2=Card
    [Header("Panels (thứ tự: Shop, Battle, Card)")]
    public RectTransform ShopPanel;
    public RectTransform BattlePanel;
    public RectTransform CardPanel;

    [Header("List Map Panel")]
    public GameObject ListMapPanel;
    public Transform mapListContent;
    public GameObject mapItemPrefab;
    public Button closeListMapButton;

    [Header("Animation")]
    public float pageWidth = 1500f;
    public float slideDuration = 0.35f;

    [Header("SFX")]
    [SerializeField] private AudioClip buttonClickSFX;

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

        openShopPanel.onClick.AddListener(() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX));
        openBattlePanel.onClick.AddListener(() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX));
        openCardPanel.onClick.AddListener(() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX));
        openListMapButton?.onClick.AddListener(() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX));
        closeListMapButton?.onClick.AddListener(() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX));

        openShopPanel.onClick.AddListener(() => GoToPanel(0, shopButtonIcon));
        openBattlePanel.onClick.AddListener(() => GoToPanel(1, battleButtonIcon));
        openCardPanel.onClick.AddListener(() => GoToPanel(2, cardButtonIcon));

        openListMapButton?.onClick.AddListener(OpenListMapPanel);
        closeListMapButton?.onClick.AddListener(CloseListMapPanel);

        ListMapPanel?.SetActive(false);

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

    // ── List Map Panel ──

    [Header("List Map Panel Animation")]
    public float panelScaleDuration = 0.3f;

    private Coroutine _panelScaleCoroutine;

    private void OpenListMapPanel()
    {
        if (ListMapPanel == null) return;
        PopulateMapList();
        ListMapPanel.SetActive(true);

        if (_panelScaleCoroutine != null) StopCoroutine(_panelScaleCoroutine);
        _panelScaleCoroutine = StartCoroutine(ScalePanel(Vector3.zero, Vector3.one));
    }

    private void CloseListMapPanel()
    {
        if (ListMapPanel == null) return;

        if (_panelScaleCoroutine != null) StopCoroutine(_panelScaleCoroutine);
        _panelScaleCoroutine = StartCoroutine(ScalePanel(
            ListMapPanel.transform.localScale, Vector3.zero,
            onDone: () => ListMapPanel.SetActive(false)
        ));
    }

    private IEnumerator ScalePanel(Vector3 from, Vector3 to, System.Action onDone = null)
    {
        var rt = ListMapPanel.transform;
        rt.localScale = from;
        float elapsed = 0f;
        while (elapsed < panelScaleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseOutBack(Mathf.Clamp01(elapsed / panelScaleDuration));
            rt.localScale = Vector3.LerpUnclamped(from, to, t);
            yield return null;
        }
        rt.localScale = to;
        onDone?.Invoke();
    }

    // Ease out back: scale vượt quá 1 rồi về đúng — cảm giác "pop" khi mở
    // Khi đóng dùng từ scale hiện tại về 0 nên chỉ cần EaseInBack
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private void PopulateMapList()
    {
        if (mapListContent == null || mapItemPrefab == null) return;

        // Xóa item cũ
        foreach (Transform child in mapListContent)
            Destroy(child.gameObject);

        var pdm = PlayerDataManager.Instance;
        if (pdm == null || pdm.allMapData == null) return;

        List<Map> allMaps = pdm.allMapData.maps;
        for (int i = 0; i < allMaps.Count; i++)
        {
            Map map = allMaps[i];

            // index dùng để tính lẻ/chẵn — dùng int.Parse(map.id) để đúng với id "1","2","3"...
            int.TryParse(map.id, out int mapIdInt);

            MapProgress progress = pdm.GetMapProgress(map.id);

            var go   = Instantiate(mapItemPrefab, mapListContent);
            var item = go.GetComponent<MapItemPrefab>();
            item?.Setup(map, progress, mapIdInt);
        }
    }
}
