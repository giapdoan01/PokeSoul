using UnityEngine;

/// <summary>
/// Ép cấu hình frame pacing sớm nhất có thể để mobile không bị kẹt dưới 60fps
/// chỉ vì vSync hoặc targetFrameRate bị reset bởi scene/manager khác.
///
/// Lưu ý: máy/màn hình chỉ có thể render cao hơn 60fps nếu thiết bị hỗ trợ
/// refresh rate lớn hơn 60Hz.
/// </summary>
[DefaultExecutionOrder(-10000)]
public sealed class FrameRateBootstrapper : MonoBehaviour
{
    private const int TargetFrameRate = 120;
    private static FrameRateBootstrapper _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureInstance()
    {
        if (_instance != null) return;

        var go = new GameObject(nameof(FrameRateBootstrapper));
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<FrameRateBootstrapper>();
    }

    private void Awake()
    {
        ApplyFrameRateSettings();
    }

    private void OnEnable()
    {
        ApplyFrameRateSettings();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            ApplyFrameRateSettings();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause)
            ApplyFrameRateSettings();
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    private static void ApplyFrameRateSettings()
    {
#if UNITY_ANDROID || UNITY_IOS
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFrameRate;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
    }
}
