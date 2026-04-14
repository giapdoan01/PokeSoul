using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class EnemyHPUI : MonoBehaviour
{
   public EnemyHPController enemyHPController;
   public Slider hpSlider;

    void Start()
    {
        enemyHPController.onEnemyHealthChanged += UpdateHPUI;
        hpSlider.maxValue = (float)enemyHPController.currentHP;
    }
    void UpdateHPUI(double hp)
    {
        hpSlider.value = (float)hp;
    }
}