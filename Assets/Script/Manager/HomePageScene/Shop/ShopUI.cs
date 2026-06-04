using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public ShopData shopData;
    public Transform cardContainer;
    public GameObject landItemPrefab;
    public PopupGachaCard popupGachaCard;
    public ShopManager shopManager;

    private void Start()
    {
        popupGachaCard.Init(shopManager);
        SetupLands();
    }

    private void SetupLands()
    {
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        foreach (var land in shopData.lands)
        {
            var item = Instantiate(landItemPrefab, cardContainer).GetComponent<LandItemPrefab>();
            item.Setup(land, popupGachaCard);
        }
    }
}
