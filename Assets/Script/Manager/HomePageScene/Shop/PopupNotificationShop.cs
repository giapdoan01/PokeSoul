using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class PopupNotificationShop : MonoBehaviour
{
    public Button closeButton;
    public Image MonImage;
    public Sprite GemSprite;
    public TMP_Text notificationText;
    public ParticleSystem successEffect;
    public ParticleSystem failureEffect;

    [Header("SFX")]
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failureSound;

    private const float AnimDuration = 0.25f;
    private Vector2 _monImageOriginalSize;

    void Start()
    {
        _monImageOriginalSize = MonImage.rectTransform.sizeDelta;
        gameObject.SetActive(false);
        closeButton.onClick.AddListener(() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX));
        closeButton.onClick.AddListener(Hide);
    }

    public void SetupNotificationBuyMonSuccess(PokemonData pokemonData, string message)
    {
        MonImage.sprite = pokemonData.spritePokemonCard;
        MonImage.rectTransform.sizeDelta = _monImageOriginalSize;
        notificationText.text = message;
        Show();
        successEffect.Play();
        SoundUIManager.Instance?.PlayUISound(successSound);
    }

    public void SetupNotificationNotEnoughGem(string message)
    {
        MonImage.sprite = GemSprite;
        MonImage.rectTransform.sizeDelta = new Vector2(250f, 250f);
        notificationText.text = message;
        Show();
        failureEffect.Play();
        SoundUIManager.Instance?.PlayUISound(failureSound);
    }

    private void Show()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(ScaleAnim(Vector3.zero, Vector3.one));
    }

    private void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleAndDeactivate());
    }

    private IEnumerator ScaleAnim(Vector3 from, Vector3 to)
    {
        float t = 0f;
        transform.localScale = from;
        while (t < AnimDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.LerpUnclamped(from, to, EaseOutBack(t / AnimDuration));
            yield return null;
        }
        transform.localScale = to;
    }

    private IEnumerator ScaleAndDeactivate()
    {
        yield return ScaleAnim(Vector3.one, Vector3.zero);
        gameObject.SetActive(false);
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
