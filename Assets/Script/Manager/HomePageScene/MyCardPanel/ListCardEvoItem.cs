using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class ListCardEvoItem : MonoBehaviour
{
    public Transform containerEvo;
    public GameObject cardEvoItemPrefab;

    public void SetupListEvo(PokemonData pokemonData)
    {
        ClearListEvo();

        PokemonData currentPokemon = pokemonData;
        while (currentPokemon != null)
        {
            GameObject evoItemObj = Instantiate(cardEvoItemPrefab, containerEvo);
            CardEvoItem evoItem = evoItemObj.GetComponent<CardEvoItem>();
            evoItem.SetupItem(currentPokemon);

            currentPokemon = currentPokemon.EvolutionPokemonData;
        }
    }
    private void ClearListEvo()
    {
        foreach (Transform child in containerEvo)
        {
            Destroy(child.gameObject);
        }
    }
}