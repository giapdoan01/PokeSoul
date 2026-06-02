using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EnemyHPController : MonoBehaviour
{
    private EnemyData enemyData;
    private int _waveNumber;
    private PlayerStatsInBattleManager _playerStats;
    public double currentHP;
    public double maxHP;
    public Action<double> onEnemyHealthChanged;
    public Action<double> onEnemyMaxHPSet;
    public Action OnDied;

    public double CurrentHP => currentHP;
    public double MaxHP => maxHP;

    [Header("SFX")]
    public AudioClip spawnSFX;
    public AudioClip dieSFX;
    public AudioSource audioSource;

    public void SetEnemyData(EnemyData data)
    {
        enemyData = data;
    }

    public void PlaySpawnSFX()
    {
        if (spawnSFX != null && audioSource != null)
            audioSource.PlayOneShot(spawnSFX);
    }

    public void SetPlayerStats(PlayerStatsInBattleManager playerStats, int waveNumber)
    {
        _playerStats = playerStats;
        _waveNumber = waveNumber;
    }

    public void setHpByWaveName(int waveName)
    {
        EnemyWaveData waveData = enemyData.getEnemyWaveDataByName(waveName);
        if (waveData != null)
        {
            maxHP = waveData.enemyStats.HP;
            currentHP = maxHP;
            Debug.Log($"[EnemyHealtController] Đã thiết lập HP cho {enemyData.enemyName} ở wave {waveName}: {currentHP}");

            // Thông báo maxHP để UI setup slider trước
            onEnemyMaxHPSet?.Invoke(maxHP);
            // Kích hoạt sự kiện onEnemyHealthChanged để UI cập nhật
            onEnemyHealthChanged?.Invoke(currentHP);
        }
        else
        {
            Debug.LogError($"[EnemyHealtController] Không thể thiết lập HP cho {enemyData.enemyName} vì không tìm thấy dữ liệu wave: {waveName}");
        }
    }

    public void TakeDamage(double damage)
    {
        currentHP -= damage;
        if (currentHP < 0)
        {
            currentHP = 0;
        }

        onEnemyHealthChanged?.Invoke(currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{enemyData.enemyName} đã chết!");

        if (dieSFX != null)
            BattleSoundManager.Instance?.PlaySound(dieSFX);

        var waveData = enemyData.getEnemyWaveDataByName(_waveNumber);
        if (waveData != null && _playerStats != null)
            _playerStats.AddCoin(waveData.enemyStats.reward);

        onEnemyHealthChanged = null;
        onEnemyMaxHPSet = null;
        _playerStats = null;
        OnDied?.Invoke();
        OnDied = null;

        MatchTracker.Instance?.RegisterEnemyDied();

        if (EnemyObjectPool.Instance != null)
            EnemyObjectPool.Instance.Return(gameObject);
        else
            Destroy(gameObject);
    }

    public void ResetHP()
    {
        currentHP = maxHP;
    }
}
