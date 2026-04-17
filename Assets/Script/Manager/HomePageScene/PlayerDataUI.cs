using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PlayerDataUI : MonoBehaviour
{
    public TMP_Text gemText;
    public TMP_Text playerNameText;
    void Start()
    {
        PlayerDataManager.Instance.OnGemChanged += UpdateGemText;

        if (PlayerDataManager.Instance.CurrentData != null)
            UpdateGemText(PlayerDataManager.Instance.CurrentData.gem);
            playerNameText.text = PlayerDataManager.Instance.CurrentData.username;
    }
    public void UpdateGemText(int gem)
    {
        gemText.text = gem.ToString();
    }
}