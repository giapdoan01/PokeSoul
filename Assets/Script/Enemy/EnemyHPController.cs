using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EnemyHPController : MonoBehaviour
{
    public EnemyData enemyData;
    public double currentHP;
    public Action<double> onEnemyHealthChanged;

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
            currentHP = waveData.enemyStats.HP;
            Debug.Log($"[EnemyHealtController] Đã thiết lập HP cho {enemyData.enemyName} ở wave {waveName}: {currentHP}");
            
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
        if (currentHP <= 0)
        {
            Die();
        }
        onEnemyHealthChanged?.Invoke(currentHP);
    }

    private void Die()
    {
        Debug.Log($"{enemyData.enemyName} đã chết!");
        Destroy(gameObject);
    }
}