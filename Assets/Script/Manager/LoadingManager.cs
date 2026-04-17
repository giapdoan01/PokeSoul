using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Threading.Tasks;

/// <summary>
/// Singleton hiển thị loading overlay khi thực hiện các tác vụ bất đồng bộ (CloudSave, network...).
/// Gọi: await LoadingManager.Instance.RunWithLoadingAsync(async () => { ... });
/// </summary>
public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject loadingOverlay;
    public TMP_Text loadingMessageText;

    [Header("Animation")]
    [Range(0.05f, 0.5f)] public float animDuration = 0.2f;

    private RectTransform _overlayRect;
    private Coroutine _animCoroutine;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        if (loadingOverlay != null)
        {
            _overlayRect = loadingOverlay.GetComponent<RectTransform>();
            loadingOverlay.SetActive(false);
        }
    }

    /// <summary>
    /// Chạy một tác vụ async và tự động show/hide loading overlay.
    /// Trả về true nếu thành công, false nếu có exception.
    /// </summary>
    public async Task<bool> RunWithLoadingAsync(Func<Task> task, string message = "Đang xử lý...")
    {
        ShowLoading(message);
        try
        {
            await task();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LoadingManager] Task failed: {e.Message}");
            return false;
        }
        finally
        {
            await HideLoadingAsync();
        }
    }

    public void ShowLoading(string message = "Đang xử lý...")
    {
        if (loadingMessageText != null)
            loadingMessageText.text = message;

        if (loadingOverlay == null) return;

        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        loadingOverlay.SetActive(true);
        _animCoroutine = StartCoroutine(ScaleAnim(Vector3.zero, Vector3.one));
    }

    public void HideLoading()
    {
        if (loadingOverlay == null) return;
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(ScaleAnim(Vector3.one, Vector3.zero, deactivateOnDone: true));
    }

    // Dùng khi cần await hide xong mới tiếp tục (VD: trong RunWithLoadingAsync)
    public Task HideLoadingAsync()
    {
        if (loadingOverlay == null) return Task.CompletedTask;
        var tcs = new TaskCompletionSource<bool>();
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(ScaleAnim(Vector3.one, Vector3.zero, deactivateOnDone: true, onDone: () => tcs.SetResult(true)));
        return tcs.Task;
    }

    // Ease out cubic: nhanh lúc đầu, chậm dần cuối
    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

    // Ease in cubic: chậm lúc đầu, nhanh dần cuối (dùng khi hide)
    private static float EaseInCubic(float t) => t * t * t;

    private IEnumerator ScaleAnim(Vector3 from, Vector3 to, bool deactivateOnDone = false, Action onDone = null)
    {
        if (_overlayRect == null) yield break;

        bool isShowing = to == Vector3.one;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);
            float eased = isShowing ? EaseOutCubic(t) : EaseInCubic(t);
            _overlayRect.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }

        _overlayRect.localScale = to;

        if (deactivateOnDone)
            loadingOverlay.SetActive(false);

        onDone?.Invoke();
    }
}
