using UnityEngine;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public bool IsOwned(string cardId) =>
        PlayerDataManager.Instance.CurrentData.ownCard.Contains(cardId);

    public PokemonData PickGachaResult(LandShopData land)
    {
        var unowned = land.pokemons.Where(p => !IsOwned(p.id)).ToList();
        if (unowned.Count == 0) return null;
        return unowned[Random.Range(0, unowned.Count)];
    }
}
