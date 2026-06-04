using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public class PopupGachaCard : MonoBehaviour
{
    [Header("UI")]
    public Transform cardContainer;
    public Button gachaButton;
    public Button closeButton;
    public TMP_Text costText;
    public PopupNotificationShop notification;

    [Header("Prefab")]
    public GameObject cardItemPrefab;

    [Header("Gacha Animation")]
    public float blinkDuration = 5f;
    public float blinkInterval = 0.12f;

    [Header("SFX")]
    public AudioClip buttonClickSFX;
    public AudioClip gachaSFX;

    private LandShopData _currentLand;
    private ShopManager _shopManager;
    private readonly List<CardShopItemPrefab> _cards = new();
    private bool _isGaching;
    private PopupScaleAnim _anim;

    private void Awake()
    {
        _anim = GetComponent<PopupScaleAnim>();
        gameObject.SetActive(false);
        closeButton.onClick.AddListener(() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX));
        closeButton.onClick.AddListener(Close);
        gachaButton.onClick.AddListener(() => SoundUIManager.Instance?.PlayUISound(buttonClickSFX));
        gachaButton.onClick.AddListener(OnGachaClicked);
    }

    public void Init(ShopManager shopManager)
    {
        _shopManager = shopManager;
    }

    public void Open(LandShopData land)
    {
        _currentLand = land;
        costText.text = $"{land.gemPerGacha}";
        SpawnCards();
        _anim ??= GetComponent<PopupScaleAnim>();
        _anim.Open();
    }

    private void Close()
    {
        if (_isGaching) return;
        _anim.Close();
    }

    private void SpawnCards()
    {
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);
        _cards.Clear();

        foreach (var pokemon in _currentLand.pokemons)
        {
            var card = Instantiate(cardItemPrefab, cardContainer).GetComponent<CardShopItemPrefab>();
            card.Setup(pokemon, _shopManager.IsOwned(pokemon.id));
            _cards.Add(card);
        }
    }

    private void OnGachaClicked()
    {
        if (_isGaching) return;
        GachaFlowAsync().Forget();
    }

    private async UniTaskVoid GachaFlowAsync()
    {
        var pdm = PlayerDataManager.Instance;

        // Kiểm tra gem
        if (pdm.CurrentData.gem < _currentLand.gemPerGacha)
        {
            notification.SetupNotificationNotEnoughGem("Không đủ Gem!");
            return;
        }

        // Lấy các thẻ chưa sở hữu
        var unownedCards = _cards.Where(c => !c.IsOwned).ToList();
        if (unownedCards.Count == 0)
        {
            notification.SetupNotificationNotEnoughGem("Đã sở hữu tất cả Mon trong vùng đất này!");
            return;
        }

        _isGaching = true;
        gachaButton.interactable = false;

        // Xác định kết quả ngay từ đầu
        var winner = _shopManager.PickGachaResult(_currentLand);

        // Save + Animation chạy song song
        SoundUIManager.Instance?.PlayLoopSound(gachaSFX);
        var saveTask = SaveGachaResultAsync(pdm, winner);
        await BlinkAnimationAsync(unownedCards);
        SoundUIManager.Instance?.StopLoopSound();

        // Chờ save hoàn tất
        bool saved = await saveTask;
        if (!saved)
        {
            ResetHighlights();
            _isGaching = false;
            gachaButton.interactable = true;
            return;
        }

        // Highlight thẻ trúng
        var winnerCard = _cards.FirstOrDefault(c => c.PokemonData.id == winner.id);
        winnerCard?.SetHighlight(true);

        // Hiện kết quả
        notification.SetupNotificationBuyMonSuccess(winner, $"Trúng!\n{winner.PokemonName}");

        // Refresh trạng thái owned
        foreach (var card in _cards)
            card.RefreshOwnedState(_shopManager.IsOwned(card.PokemonData.id));

        winnerCard?.SetHighlight(false);

        _isGaching = false;
        gachaButton.interactable = true;
    }

    private async UniTask<bool> SaveGachaResultAsync(PlayerDataManager pdm, PokemonData winner)
    {
        return await LoadingManager.Instance.RunWithLoadingAsync(async () =>
        {
            await pdm.MinusGemAsync(_currentLand.gemPerGacha);
            await pdm.AddCardToOwnCardAsync(winner.id);
        }, $"Đang thực hiện gacha...");
    }

    private async UniTask BlinkAnimationAsync(List<CardShopItemPrefab> cards)
    {
        float elapsed = 0f;
        CardShopItemPrefab current = null;

        while (elapsed < blinkDuration)
        {
            current?.SetHighlight(false);
            current = cards[Random.Range(0, cards.Count)];
            current.SetHighlight(true);

            await UniTask.Delay((int)(blinkInterval * 1000));
            elapsed += blinkInterval;
        }

        current?.SetHighlight(false);
    }

    private void ResetHighlights()
    {
        foreach (var card in _cards)
            card.SetHighlight(false);
    }
}
