using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PopupNotificationShop : MonoBehaviour
{
    public Button closeButton;
    public Image MonImage;
    public Sprite GemSprite;
    public TMP_Text notificationText;
    public ParticleSystem successEffect;
    public ParticleSystem failureEffect;

    void Start()
    {
        gameObject.SetActive(false);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }
    public void SetupNotificationBuyMonSuccess(PokemonData pokemonData,string message)
    {
        MonImage.sprite = pokemonData.spritePokemonCard;
        notificationText.text = message;
        successEffect.Play();
    }
    public void SetupNotificationNotEnoughGem(string message)
    {
        MonImage.sprite = GemSprite;
        notificationText.text = message;
        failureEffect.Play();
    }
}
