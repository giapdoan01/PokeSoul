using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class SetupListCardEvoContainer : MonoBehaviour
{
    public Transform containerListEvo;
    public GameObject listCardEvoItemPrefab;
    public AllPokemonData allPokemonData;

    void OnEnable() {
        PlayerDataManager playerDataManager = PlayerDataManager.Instance;
        if (playerDataManager != null) {
           playerDataManager.OnPlayerDataLoaded += OnPlayerDataLoaded;
           if (playerDataManager.CurrentData != null) {
               OnPlayerDataLoaded();
           }
        }
    }
    void OnDisable() {
        PlayerDataManager playerDataManager = PlayerDataManager.Instance;
        if (playerDataManager != null) {
           playerDataManager.OnPlayerDataLoaded -= OnPlayerDataLoaded;
        }
    }

    public void OnPlayerDataLoaded() {
        List<string> ownCard = PlayerDataManager.Instance.CurrentData.GetOwnCard();
        if (ownCard != null) {
            SetupListEvoContainer(ownCard.Select(cardId => allPokemonData.GetPokemonDataById(cardId)).Where(data => data != null).ToArray());
        }
    }

    public void SetupListEvoContainer(PokemonData[] ownPokemonData)
    {
        ClearListEvoContainer();

        foreach (PokemonData pokemonData in ownPokemonData)
        {
            GameObject listEvoItemObj = Instantiate(listCardEvoItemPrefab, containerListEvo);
            ListCardEvoItem listEvoItem = listEvoItemObj.GetComponent<ListCardEvoItem>();
            listEvoItem.SetupListEvo(pokemonData);
        }
    }
    private void ClearListEvoContainer()
    {
        foreach (Transform child in containerListEvo)
        {
            Destroy(child.gameObject);
        }
    }
}