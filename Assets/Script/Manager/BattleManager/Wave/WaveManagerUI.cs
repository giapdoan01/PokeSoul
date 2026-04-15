using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;


public class WaveManagerUI : MonoBehaviour {
    public Button nextWaveButton;
    public TMP_Text timeToStartFirstWaveText;
    public WaveManager waveManager;

    void Awake() {
        nextWaveButton.onClick.AddListener(OnNextWaveButtonClicked);
        waveManager.OnCountdownTick += UpdateCountdownText;
        waveManager.OnCountdownFinished += HideCountdownText;
        timeToStartFirstWaveText.gameObject.SetActive(false);
    }

    private void OnNextWaveButtonClicked() {
        waveManager.StartNextWave();
    }

    private void UpdateCountdownText(int time) {
        timeToStartFirstWaveText.gameObject.SetActive(true);
        timeToStartFirstWaveText.text = time.ToString();
    }

    private void HideCountdownText() {
        timeToStartFirstWaveText.gameObject.SetActive(false);
    }
}