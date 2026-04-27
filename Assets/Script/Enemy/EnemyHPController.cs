using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EnemyHPController : MonoBehaviour
{
    private EnemyData enemyData;
    public double currentHP;
    public double maxHP;
    public Action<double> onEnemyHealthChanged;
    public Action<double> onEnemyMaxHPSet;

    public double CurrentHP => currentHP;
    public double MaxHP => maxHP;

    public void SetEnemyData(EnemyData data)
    {
        enemyData = data;
    }

    void Awake()
    {

    }
    
    private void Start()
    {

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
        Destroy(gameObject);
        
    }
}
