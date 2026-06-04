using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LandItemPrefab : MonoBehaviour
{
    public Image backgroundImage;
    public TMP_Text landNameText;
    public Button openGachaButton;

    [Header("SFX")]
    [SerializeField] private AudioClip buttonClickSFX;

    public void Setup(LandShopData land, PopupGachaCard popup)
    {
        if (land.backgroundSprite != null)
            backgroundImage.sprite = land.backgroundSprite;

        landNameText.text = land.landName;
        openGachaButton.onClick.AddListener(() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX));
        openGachaButton.onClick.AddListener(() => popup.Open(land));
    }
}
