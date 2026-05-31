using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SettingsHomePopup : MonoBehaviour
{
    [Header("Audio Groups")]
    public AudioMixerGroup bgmGroup;
    public AudioMixerGroup sfxGroup;

    [Header("Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Buttons")]
    public Button logoutButton;
    public Button closeButton;

    [Header("SFX")]
    public AudioClip buttonClickSFX;

    private const string BGMParam = "BGMVolume";
    private const string SFXParam = "SFXUIVolume";
    private const string PrefsBGM = "settings_bgm";
    private const string PrefsSFX = "settings_sfx";
    private const string AuthScene = "AuthScene";

    private void Awake()
    {
        gameObject.SetActive(false);

        bgmSlider.value = PlayerPrefs.GetFloat(PrefsBGM, 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat(PrefsSFX, 0.75f);

        ApplyVolume(bgmGroup, BGMParam, bgmSlider.value);
        ApplyVolume(sfxGroup, SFXParam, sfxSlider.value);

        bgmSlider.onValueChanged.AddListener(v =>
        {
            ApplyVolume(bgmGroup, BGMParam, v);
            PlayerPrefs.SetFloat(PrefsBGM, v);
        });
        sfxSlider.onValueChanged.AddListener(v =>
        {
            ApplyVolume(sfxGroup, SFXParam, v);
            PlayerPrefs.SetFloat(PrefsSFX, v);
        });

        logoutButton.onClick.AddListener(OnLogout);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));

        logoutButton.onClick.AddListener(PlayClick);
        closeButton.onClick.AddListener(PlayClick);
    }

    public void Open() => gameObject.SetActive(true);

    private static void ApplyVolume(AudioMixerGroup group, string param, float sliderValue)
    {
        if (group == null) return;
        float dB = sliderValue > 0.001f ? Mathf.Log10(sliderValue) * 20f : -80f;
        group.audioMixer.SetFloat(param, dB);
    }

    private void OnLogout()
    {
        AuthManager.Instance?.Logout();
        SceneManager.LoadScene(AuthScene);
    }

    private void PlayClick() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX);
}
