using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class EnemyHPUI : MonoBehaviour
{
   public EnemyHPController enemyHPController;
   public Canvas hpCanvas;
   public Slider hpSlider;

   private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        enemyHPController.onEnemyMaxHPSet += SetMaxHP;
        enemyHPController.onEnemyHealthChanged += UpdateHPUI;
    }

    void LateUpdate()
    {
        if (hpCanvas != null && mainCamera != null)
        {
            hpCanvas.transform.rotation = mainCamera.transform.rotation;
        }
    }

    void SetMaxHP(double maxHP)
    {
        hpSlider.maxValue = (float)maxHP;
    }

    void UpdateHPUI(double hp)
    {
        hpSlider.value = (float)hp;
    }
}