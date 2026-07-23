using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class CardInBattleItemPrefab : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public PokemonData cardData;
    public Image cardImage;
    public Image monImage;
    public TMP_Text cardNameText;
    public TMP_Text cardCostText;
    public Image TypeImage;
    public Image RarityImage;
    public Sprite[] TypeSprites;
    public Sprite[] TypeBackgroundSprites;
    public List<Sprite> RaritySprites;

    [Header("Coin SFX")]
    public AudioClip coinSFX;
    public AudioSource coinAudioSource;

    [Header("Color Text By Type")]
    public Color fireColor;
    public Color waterColor;
    public Color grassColor;
    public Color electricColor;
    public Color psychicColor;
    public Color iceColor;
    public Color darkColor;
    public Color fightingColor;
    public Color poisonColor;
    public Color groundColor;

    // Inject từ CardDeckInBattleManager sau khi Instantiate
    private Image _dragGhostImage;
    private RectTransform _ghostRect;
    private Canvas _rootCanvas;
    private bool _isDragging;
    private PlayerStatsInBattleManager _playerStats;
    private int _cost;

    private void Awake()
    {
        _rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
    }

    // ── Setup ──

    public void SetCardData(PokemonData data)
    {
        cardData = data;
        monImage.sprite = cardData.spritePokemonCard;
        cardNameText.text = cardData.PokemonName;
        var levelData = cardData.getPokemonLevelDataByLevel(1);
        var costEntry = levelData?.GetStatEntryByName("Price");
        _cost = costEntry != null ? (int)costEntry.value : 0;
        cardCostText.text = costEntry != null ? _cost.ToString() : "?";
        if (costEntry == null)
            Debug.LogWarning($"[CardInBattleItemPrefab] '{cardData.PokemonName}' thiếu stat 'Price' ở level 1");
        SetUpTypeSprite(cardData);
        SetupTextColor(cardData.type);
        SetupRarity(cardData);
    }

    public void SetPlayerStats(PlayerStatsInBattleManager stats) => _playerStats = stats;

    // Gọi bởi CardDeckInBattleManager ngay sau Instantiate
    public void SetDragGhost(Image ghostImage)
    {
        _dragGhostImage = ghostImage;
        if (_dragGhostImage == null) return;
        _ghostRect = _dragGhostImage.GetComponent<RectTransform>();
        _dragGhostImage.raycastTarget = false;
        _dragGhostImage.gameObject.SetActive(false);
    }

    // ── Drag & Drop ──

    public void OnBeginDrag(PointerEventData e)
    {
        if (cardData == null || BattleAssetManager.Instance?.GetMonPrefab(cardData.id) == null) return;
        _isDragging = true;

        if (_dragGhostImage != null)
        {
            _dragGhostImage.sprite = monImage.sprite;
            _dragGhostImage.gameObject.SetActive(true);
            MoveGhost(e.position);
        }

        GetComponent<CanvasGroup>()?.Let(cg => cg.alpha = 0.45f);
    }

    public void OnDrag(PointerEventData e)
    {
        if (!_isDragging) return;
        MoveGhost(e.position);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!_isDragging) return;
        _isDragging = false;

        _dragGhostImage?.gameObject.SetActive(false);
        GetComponent<CanvasGroup>()?.Let(cg => cg.alpha = 1f);

        TryPlaceMon(e.position);
    }

    private void MoveGhost(Vector2 screenPos)
    {
        if (_ghostRect == null || _rootCanvas == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rootCanvas.GetComponent<RectTransform>(),
            screenPos,
            _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera,
            out Vector2 local
        );
        _ghostRect.localPosition = local;
    }

    private void TryPlaceMon(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Kiểm tra coin trước khi tìm slot
        if (_playerStats != null && _playerStats.playerCoin < _cost)
        {
            Debug.Log($"[Card] Không đủ coin. Cần {_cost}, hiện có {_playerStats.playerCoin}");
            return;
        }

        Ray ray = cam.ScreenPointToRay(screenPos);
        Vector3 worldPos = Physics.Raycast(ray, out RaycastHit hit, 100f)
            ? hit.point
            : ray.GetPoint(10f);

        PlacementSlot slot = PlacementSlotRegistry.FindNearestFree(worldPos, maxDistance: 5f);
        if (slot == null) return;

        if (slot.TrySpawn(cardData))
        {
            _playerStats?.MinusCoin(_cost);
            if (coinSFX != null && coinAudioSource != null)
                coinAudioSource.PlayOneShot(coinSFX);
        }
    }

    // ── Visual helpers ──

    private void SetupTextColor(PokemonType type)
    {
        Color color = type switch
        {
            PokemonType.Fire     => fireColor,
            PokemonType.Water    => waterColor,
            PokemonType.Grass    => grassColor,
            PokemonType.Electric => electricColor,
            PokemonType.Psychic  => psychicColor,
            PokemonType.Ice      => iceColor,
            PokemonType.Dark     => darkColor,
            PokemonType.Fighting => fightingColor,
            PokemonType.Poison   => poisonColor,
            PokemonType.Ground   => groundColor,
            _                    => Color.white
        };
        cardNameText.color = color;
    }
    public void SetupRarity(PokemonData data)
    {
        switch (data.Rarity)
        {
            case "C":
                RarityImage.sprite = RaritySprites[0];
                break;
            case "R":
                RarityImage.sprite = RaritySprites[1];
                break;
            case "S":
                RarityImage.sprite = RaritySprites[2];
                break;
            case "SSR":
                RarityImage.sprite = RaritySprites[3];
                break;
            case "SSS":
                RarityImage.sprite = RaritySprites[4];
                break;
            default:
                Debug.LogWarning($"[CardInBattleItemPrefab] Unknown rarity: {data.Rarity}");
                break;
        }
    }

    public void SetUpTypeSprite(PokemonData data)
    {
        int idx = (int)data.type;
        if (idx < TypeSprites.Length)           TypeImage.sprite  = TypeSprites[idx];
        if (idx < TypeBackgroundSprites.Length) cardImage.sprite  = TypeBackgroundSprites[idx];
    }
}

internal static class CanvasGroupExt
{
    internal static void Let(this CanvasGroup cg, Action<CanvasGroup> action) => action(cg);
}
