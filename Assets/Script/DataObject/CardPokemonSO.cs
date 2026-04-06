using UnityEngine;

public enum PokemonType
{
    Fire,
    Water,
    Grass,
    Electric,
    Psychic,
    Ice,
    Dark,
    Fighting,
    Poison,
    Ground
}

[CreateAssetMenu(fileName = "NewPokemonCard", menuName = "PokeSoul/Pokemon Card")]
public class CardPokemonSO : ScriptableObject
{
    [Header("Info")]
    public string id;
    public string pokemonName;
    public PokemonType type;

    [Header("Visual")]
    public Sprite spritePokemonCard;
    public GameObject fxTypeCard;

    [Header("Shop")]
    public int priceToBuy;
}
