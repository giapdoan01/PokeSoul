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

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        BindController();
        RefreshUIInstant();
    }

    private void Start()
    {
        RefreshUIInstant();
    }

    private void OnDisable()
    {
        UnbindController();
    }

    private void OnDestroy()
    {
        UnbindController();
    }

    private void BindController()
    {
        if (enemyHPController == null)
        {
            enemyHPController = GetComponentInParent<EnemyHPController>();
        }

        if (enemyHPController == null)
        {
            Debug.LogWarning("[EnemyHPUI] enemyHPController is null.", this);
            return;
        }

        enemyHPController.onEnemyMaxHPSet -= SetMaxHP;
        enemyHPController.onEnemyHealthChanged -= UpdateHPUI;
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
        if (hpSlider == null)
        {
            return;
        }

        hpSlider.maxValue = (float)maxHP;
    }

    void UpdateHPUI(double hp)
    {
        if (hpSlider == null)
        {
            return;
        }

        hpSlider.value = (float)hp;
    }

    private void RefreshUIInstant()
    {
        if (enemyHPController == null || hpSlider == null)
        {
            return;
        }

        float maxHpValue = enemyHPController.MaxHP > 0 ? (float)enemyHPController.MaxHP : (float)enemyHPController.CurrentHP;
        hpSlider.maxValue = maxHpValue;
        hpSlider.value = (float)enemyHPController.CurrentHP;
    }

    private void UnbindController()
    {
        if (enemyHPController == null)
        {
            return;
        }

        enemyHPController.onEnemyMaxHPSet -= SetMaxHP;
        enemyHPController.onEnemyHealthChanged -= UpdateHPUI;
    }
}
