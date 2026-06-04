using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EnemyHPController : MonoBehaviour
{
    private EnemyData enemyData;
    private int _reward;
    private PlayerStatsInBattleManager _playerStats;
    public double currentHP;
    public double maxHP;
    public Action<double> onEnemyHealthChanged;
    public Action<double> onEnemyMaxHPSet;
    public Action OnDied;

    public double CurrentHP => currentHP;
    public double MaxHP => maxHP;

    [Header("VFX")]
    public GameObject bloodVFX;

    [Header("SFX")]
    public AudioClip spawnSFX;
    public AudioClip dieSFX;
    public AudioSource audioSource;

    private const string BloodVFXPoolKey = "BloodVFX";
    private const float BloodVFXLifeTime = 1.5f;
    private const float BloodVFXYPos = 0.16f;

    public void SetEnemyData(EnemyData data)
    {
        enemyData = data;
    }

    public void PlaySpawnSFX()
    {
        if (spawnSFX != null && audioSource != null)
            audioSource.PlayOneShot(spawnSFX);
    }

    private void Awake()
    {
        if (bloodVFX != null && EnemyObjectPool.Instance != null)
        {
            // Đảm bảo VFX có component auto-return
            if (bloodVFX.GetComponent<BloodVFXAutoReturn>() == null)
                bloodVFX.AddComponent<BloodVFXAutoReturn>().lifeTime = BloodVFXLifeTime;

            EnemyObjectPool.Instance.RegisterEnemy(BloodVFXPoolKey, bloodVFX);
        }
    }

    public void SetPlayerStats(PlayerStatsInBattleManager playerStats, int reward)
    {
        _playerStats = playerStats;
        _reward = reward;
    }

    public void SetHp(double hp)
    {
        maxHP = hp;
        currentHP = maxHP;
        onEnemyMaxHPSet?.Invoke(maxHP);
        onEnemyHealthChanged?.Invoke(currentHP);
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

        SpawnBloodVFX();

        if (_playerStats != null && _reward > 0)
            _playerStats.AddCoin(_reward);

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

    // Gọi khi enemy chạy tới đích (không bị giết) — để WaveManager biết
    public void OnEscaped()
    {
        onEnemyHealthChanged = null;
        onEnemyMaxHPSet = null;
        _playerStats = null;
        OnDied?.Invoke();  // Notify WaveManager giống như die
        OnDied = null;
        // Không cộng reward, không RegisterEnemyDied (đã xử lý ở RegisterEnemyReachedEnd)
    }

    private void SpawnBloodVFX()
    {
        if (bloodVFX == null || EnemyObjectPool.Instance == null) return;
        var vfxPos = new Vector3(transform.position.x, BloodVFXYPos, transform.position.z);
        EnemyObjectPool.Instance.GetOrRegister(BloodVFXPoolKey, bloodVFX, vfxPos, Quaternion.identity);
    }

    public void ResetHP()
    {
        currentHP = maxHP;
    }
}
