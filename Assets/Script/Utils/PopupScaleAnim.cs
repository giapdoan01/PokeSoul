using UnityEngine;
using System.Collections;

// Gắn vào bất kỳ popup nào cần hiệu ứng scale open/close.
// Gọi Open() để mở, Close() để đóng.
[RequireComponent(typeof(RectTransform))]
public class PopupScaleAnim : MonoBehaviour
{
    [SerializeField] private float duration = 0.3f;

    private Coroutine _coroutine;

    private void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        PlayAnim(Vector3.zero, Vector3.one);
    }

    public void Close(System.Action onDone = null)
    {
        PlayAnim(transform.localScale, Vector3.zero, () =>
        {
            gameObject.SetActive(false);
            onDone?.Invoke();
        });
    }

    private void PlayAnim(Vector3 from, Vector3 to, System.Action onDone = null)
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(Scale(from, to, onDone));
    }

    private IEnumerator Scale(Vector3 from, Vector3 to, System.Action onDone)
    {
        transform.localScale = from;
        float elapsed = 0f;
        bool opening = to == Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = opening ? EaseOutBack(t) : EaseInCubic(t);
            transform.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }

        transform.localScale = to;
        onDone?.Invoke();
    }

    // Bật lên có cảm giác "pop" vui
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // Tắt nhanh gọn
    private static float EaseInCubic(float t) => t * t * t;
}
