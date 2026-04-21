using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PopupListCardSelect : MonoBehaviour
{
    public GameObject popupListCardSelect;
    public CardDeckInPopupSelectItemPrefab cardDeckInPopupSelectItemPrefab;
    public Transform container;
    public Button closeButton;

    [Header("Scale Settings")]
    [SerializeField] private float scaleDuration = 0.3f;

    private readonly AnimationCurve _easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    private Coroutine _scaleCoroutine;
    private int _currentEditingPosition = -1;

    void Start()
    {
        popupListCardSelect.transform.localScale = Vector3.zero;
        popupListCardSelect.SetActive(false);
        closeButton.onClick.AddListener(ClosePopup);
    }

    public void SetCurrentEditingPosition(int position)
    {
        _currentEditingPosition = position;
        CardDeckHomePageContainer.Instance?.SetSelectFrame(position);
    }

    public void OpenPopup()
    {
        LoadingManager.Instance.RunWithLoadingAsync(async () =>
        {
            popupListCardSelect.SetActive(true);
            LoadPlayerCards();
            PlayScale(Vector3.zero, Vector3.one, null);
        }, "Đang tải danh sách thẻ bài...");
    }

    public void ClosePopup()
    {
        CardDeckHomePageContainer.Instance?.ClearSelectFrame();
        PlayScale(Vector3.one, Vector3.zero, () =>
        {
            popupListCardSelect.SetActive(false);
            ClearContainer();
            _currentEditingPosition = -1;
        });
    }

    private void PlayScale(Vector3 from, Vector3 to, System.Action onComplete)
    {
        if (_scaleCoroutine != null)
            StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScaleRoutine(from, to, onComplete));
    }

    private IEnumerator ScaleRoutine(Vector3 from, Vector3 to, System.Action onComplete)
    {
        float elapsed = 0f;
        popupListCardSelect.transform.localScale = from;

        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = _easeCurve.Evaluate(Mathf.Clamp01(elapsed / scaleDuration));
            popupListCardSelect.transform.localScale = Vector3.LerpUnclamped(from, to, t);
            yield return null;
        }

        popupListCardSelect.transform.localScale = to;
        onComplete?.Invoke();
    }

    private void LoadPlayerCards()
    {
        ClearContainer();
        var playerData = PlayerDataManager.Instance.CurrentData;
        List<string> cardsInBattleDeck = PlayerDataManager.Instance.GetBattleDeck();

        foreach (string cardId in playerData.ownCard)
        {
            PokemonData pokemonData = PlayerDataManager.Instance.GetPokemonDataByIdFromPlayerData(cardId);
            if (pokemonData == null) continue;

            GameObject cardObj = Instantiate(cardDeckInPopupSelectItemPrefab.gameObject, container);
            CardDeckInPopupSelectItemPrefab cardItem = cardObj.GetComponent<CardDeckInPopupSelectItemPrefab>();
            cardItem.SetupCard(pokemonData, this, cardsInBattleDeck.Contains(cardId));
        }
    }

    private void ClearContainer()
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);
    }

    public void OnCardSelected(string cardId)
    {
        if (_currentEditingPosition < 0 || _currentEditingPosition >= 4)
        {
            Debug.LogError($"Invalid position: {_currentEditingPosition}");
            return;
        }

        LoadingManager.Instance.RunWithLoadingAsync(async () =>
        {
            await PlayerDataManager.Instance.AddCardToBattleDeckAsync(cardId, _currentEditingPosition);
            ClosePopup();

            if (CardDeckHomePageContainer.Instance != null)
                CardDeckHomePageContainer.Instance.RefreshDeckUI();
            else
                Debug.LogWarning("CardDeckHomePageContainer not found.");

        }, "Đang thêm thẻ bài vào deck...");
    }
}