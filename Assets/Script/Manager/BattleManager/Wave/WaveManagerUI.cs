using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveManagerUI : MonoBehaviour
{
    public Button nextWaveButton;
    public TMP_Text timeToStartFirstWaveText;
    public WaveManager waveManager;

    [Header("SFX")]
    public AudioClip buttonClickSFX;

    void Awake()
    {
        nextWaveButton.onClick.AddListener(OnNextWaveButtonClicked);
        nextWaveButton.onClick.AddListener(() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX));
        nextWaveButton.gameObject.SetActive(false);

        waveManager.OnCountdownTick      += UpdateCountdownText;
        waveManager.OnCountdownFinished  += HideCountdownText;
        waveManager.OnWaveSpawnComplete  += OnSpawnComplete;
        waveManager.OnWaveCleared        += OnWaveCleared;

        timeToStartFirstWaveText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        waveManager.OnCountdownTick     -= UpdateCountdownText;
        waveManager.OnCountdownFinished -= HideCountdownText;
        waveManager.OnWaveSpawnComplete -= OnSpawnComplete;
        waveManager.OnWaveCleared       -= OnWaveCleared;
    }

    private void OnNextWaveButtonClicked()
    {
        nextWaveButton.gameObject.SetActive(false);
        waveManager.StartNextWave();
    }

    // Spawn xong toàn bộ → hiện nút cho player bấm sớm nếu muốn
    private void OnSpawnComplete()
    {
        nextWaveButton.gameObject.SetActive(true);
    }

    // Enemy chết hết → ẩn nút (auto next wave đã được gọi từ WaveManager)
    private void OnWaveCleared()
    {
        nextWaveButton.gameObject.SetActive(false);
    }

    private void UpdateCountdownText(int time)
    {
        timeToStartFirstWaveText.gameObject.SetActive(true);
        timeToStartFirstWaveText.text = time.ToString();
    }

    private void HideCountdownText()
    {
        timeToStartFirstWaveText.text = "Trận đấu bắt đầu!";
        StartCoroutine(HideAfterDelay(1.5f));
    }

    private System.Collections.IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        timeToStartFirstWaveText.gameObject.SetActive(false);
    }
}
