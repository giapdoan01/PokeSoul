using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "NewLandShopData", menuName = "PokeSoul/Shop/Land Shop Data")]
public class LandShopData : ScriptableObject
{
    public string landName;
    public Sprite backgroundSprite;
    public int gemPerGacha;
    public PokemonData[] pokemons;
}
