using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsPopup : MonoBehaviour
{
    [Header("Audio Groups")]
    public AudioMixerGroup bgmGroup;
    public AudioMixerGroup sfxGroup;

    [Header("Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Buttons")]
    public Button quitBattleButton;
    public Button closeButton;

    [Header("SFX")]
    public AudioClip buttonClickSFX;

    private const string BGMParam = "BGMBattleVolume";
    private const string SFXParam = "SFXBattleVolume";
    private const string PrefsBGM = "settings_bgm";
    private const string PrefsSFX = "settings_sfx";

    private PopupScaleAnim _anim;

    private void Awake()
    {
        _anim = GetComponent<PopupScaleAnim>();
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

        quitBattleButton.onClick.AddListener(OnQuitBattle);
        closeButton.onClick.AddListener(() => _anim.Close());

        quitBattleButton.onClick.AddListener(PlayClick);
        closeButton.onClick.AddListener(PlayClick);
    }

    public void Open() => _anim.Open();

    private static void ApplyVolume(AudioMixerGroup group, string param, float sliderValue)
    {
        if (group == null) return;
        float dB = sliderValue > 0.001f ? Mathf.Log10(sliderValue) * 20f : -80f;
        group.audioMixer.SetFloat(param, dB);
    }

    private void OnQuitBattle()
    {
        _anim.Close(onDone: () => MatchTracker.Instance?.ForceLose());
    }

    private void PlayClick() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX);
}
