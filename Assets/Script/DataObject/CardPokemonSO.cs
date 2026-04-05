using UnityEngine;

public enum PokemonType
{
    Fire,
    Water,
    Grass,
    Electric,
    Psychic,
    Ice,
    Dragon,
    Dark,
    Fighting,
    Normal
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
