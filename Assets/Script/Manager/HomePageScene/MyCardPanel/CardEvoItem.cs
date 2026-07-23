using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class CardEvoItem : MonoBehaviour
{
    private static readonly int RarityPropertyId = Shader.PropertyToID("_Rarity");

    public Image TypeCardBackground;
    public Image MonImage;
    public TMP_Text MonName;
    public Image nextEvoIcon;
    public Image RarityImage;
    public Image TypeImage;
    public List<Sprite> TypeCardBackgroundSprites;
    public List<Sprite> TypeSprites;
    public List<Sprite> RaritySprites;

    [Header("Rarity Frame Shader")]
    public Image RarityFrameImage;
    public Material RarityFrameMaterial;

    private Material runtimeRarityFrameMaterial;
    private Material sourceRarityFrameMaterial;

    public void SetupItem(PokemonData pokemonData)
    {
        SetupType(pokemonData);
        SetupRarity(pokemonData);
        MonImage.sprite = pokemonData.spritePokemonCard;
        MonName.text = pokemonData.PokemonName;
        nextEvoIcon.gameObject.SetActive(pokemonData.EvolutionPokemonData != null);
    }
    private void SetupType(PokemonData cardPokemon)
    {
        switch (cardPokemon.type)
        {
            case PokemonType.Fire:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[0];
                TypeImage.sprite = TypeSprites[0];
                break;
            case PokemonType.Water:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[1];
                TypeImage.sprite = TypeSprites[1];
                break;
            case PokemonType.Grass:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[2];
                TypeImage.sprite = TypeSprites[2];
                break;
            case PokemonType.Electric:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[3];
                TypeImage.sprite = TypeSprites[3];
                break;
            case PokemonType.Psychic:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[4];
                TypeImage.sprite = TypeSprites[4];
                break;
            case PokemonType.Ice:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[5];
                TypeImage.sprite = TypeSprites[5];
                break;
            case PokemonType.Dark:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[6];
                TypeImage.sprite = TypeSprites[6];
                break;
            case PokemonType.Fighting:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[7];
                TypeImage.sprite = TypeSprites[7];
                break;
            case PokemonType.Poison:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[8];
                TypeImage.sprite = TypeSprites[8];
                break;
            case PokemonType.Ground:
                TypeCardBackground.sprite = TypeCardBackgroundSprites[9];
                TypeImage.sprite = TypeSprites[9];
                break;
        }
    }
    public void SetupRarity(PokemonData cardPokemon)
    {
        int rarityIndex = GetRarityIndex(cardPokemon.Rarity);
        SetRaritySprite(rarityIndex);
        ApplyRarityFrame(rarityIndex);
    }

    private void ApplyRarityFrame(int rarityIndex)
    {
        Image targetFrame = RarityFrameImage != null ? RarityFrameImage : RarityImage;
        if (targetFrame == null || RarityFrameMaterial == null)
        {
            return;
        }

        Material material = GetOrCreateRarityFrameMaterial();
        targetFrame.material = material;
        material.SetFloat(RarityPropertyId, rarityIndex);
    }

    private Material GetOrCreateRarityFrameMaterial()
    {
        if (runtimeRarityFrameMaterial != null && sourceRarityFrameMaterial == RarityFrameMaterial)
        {
            return runtimeRarityFrameMaterial;
        }

        ReleaseRuntimeRarityFrameMaterial();
        sourceRarityFrameMaterial = RarityFrameMaterial;
        runtimeRarityFrameMaterial = new Material(RarityFrameMaterial)
        {
            name = $"{RarityFrameMaterial.name} ({name})"
        };

        return runtimeRarityFrameMaterial;
    }

    private void SetRaritySprite(int rarityIndex)
    {
        if (RarityImage == null || RaritySprites == null || rarityIndex < 0 || rarityIndex >= RaritySprites.Count)
        {
            return;
        }

        RarityImage.sprite = RaritySprites[rarityIndex];
    }

    private static int GetRarityIndex(string rarity)
    {
        switch ((rarity ?? string.Empty).Trim().ToUpperInvariant())
        {
            case "R":
                return 1;
            case "S":
                return 2;
            case "SSR":
                return 3;
            case "SSS":
                return 4;
            case "C":
            default:
                return 0;
        }
    }

    private void OnDestroy()
    {
        ReleaseRuntimeRarityFrameMaterial();
    }

    private void ReleaseRuntimeRarityFrameMaterial()
    {
        if (runtimeRarityFrameMaterial == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(runtimeRarityFrameMaterial);
        }
        else
        {
            DestroyImmediate(runtimeRarityFrameMaterial);
        }

        runtimeRarityFrameMaterial = null;
        sourceRarityFrameMaterial = null;
    }
}
