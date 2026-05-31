using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class MonUpgradePanel : MonoBehaviour
{
    [Header("Info")]
    public Image monImage;
    public TMP_Text monNameText;
    public TMP_Text levelText;
    public TMP_Text damageText;
    public TMP_Text cooldownText;

    [Header("Buttons")]
    public Button upgradeButton;
    public Button evolveButton;
    public Button sellButton;
    public Button closeButton;

    [Header("Button Labels")]
    public TMP_Text upgradePriceText;
    public TMP_Text evolvePriceText;
    public TMP_Text sellValueText;

    [Header("Animation")]
    public float scaleDuration = 0.25f;

    [Header("SFX")]
    public AudioClip buttonClickSFX;

    private MonOnSlot _current;
    private Coroutine _scaleCoroutine;

    private void Awake()
    {
        upgradeButton.onClick.AddListener(OnUpgrade);
        evolveButton.onClick.AddListener(OnEvolve);
        sellButton.onClick.AddListener(OnSell);
        closeButton?.onClick.AddListener(Hide);

        upgradeButton.onClick.AddListener(PlayClick);
        evolveButton.onClick.AddListener(PlayClick);
        sellButton.onClick.AddListener(PlayClick);
        closeButton?.onClick.AddListener(PlayClick);

        transform.localScale = Vector3.zero;
    }

    private void PlayClick() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX);

    public void Show(MonOnSlot mon)
    {
        _current = mon;
        Refresh();
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScaleTo(Vector3.zero, Vector3.one));
    }

    public void Hide()
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScaleTo(transform.localScale, Vector3.zero));
    }

    private void Refresh()
    {
        if (_current == null) return;

        if (monImage != null) monImage.sprite = _current.CurrentData.spritePokemonCard;
        monNameText.text = _current.CurrentData.PokemonName;
        levelText.text = $"Lv {_current.CurrentLevel}";
        damageText.text = $"DMG: {_current.GetDamage():F1}";
        cooldownText.text = $"CD: {_current.GetCooldown():F2}s";
        sellValueText.text = $"{_current.GetSellValue()}";

        bool isMax = _current.IsMaxLevel();
        bool canEvo = isMax && _current.CurrentData.EvolutionPokemonData != null;

        // Upgrade button
        upgradeButton.gameObject.SetActive(!isMax);
        if (!isMax)
            upgradePriceText.text = $"{_current.GetUpgradePrice()}";

        // Evolve button
        evolveButton.gameObject.SetActive(canEvo);
        if (canEvo)
            evolvePriceText.text = $"{_current.GetEvoPrice()}";
    }

    private void OnUpgrade()
    {
        if (_current == null) return;
        if (_current.TryUpgrade())
            Refresh();
    }

    private void OnEvolve()
    {
        if (_current == null) return;
        if (_current.TryEvolve())
            Hide(); // slot sẽ destroy mon này, đóng panel
    }

    private void OnSell()
    {
        if (_current == null) return;
        _current.SellSelf();
        Hide();
    }

    private IEnumerator ScaleTo(Vector3 from, Vector3 to, System.Action onDone = null)
    {
        transform.localScale = from;
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / scaleDuration;
            // ease out back khi mở, ease in khi đóng
            float eased = to == Vector3.zero
                ? t * t * t
                : 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
            transform.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }
        transform.localScale = to;
        onDone?.Invoke();
    }
}
