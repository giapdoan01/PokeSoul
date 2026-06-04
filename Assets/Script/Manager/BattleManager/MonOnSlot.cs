using UnityEngine;
public class MonOnSlot : MonoBehaviour
{
    public PokemonData CurrentData { get; private set; }
    public int CurrentLevel { get; private set; }

    private PlacementSlot _slot;
    private PlayerStatsInBattleManager _playerStats;
    private MonUpgradePanel _upgradePanel;
    private PokemonSkill _skill;

    public void Init(PokemonData data, PlacementSlot slot, PlayerStatsInBattleManager playerStats, MonUpgradePanel upgradePanel)
    {
        CurrentData = data;
        CurrentLevel = 1;
        _slot = slot;
        _playerStats = playerStats;
        _upgradePanel = upgradePanel;
        _skill = GetComponentInChildren<PokemonSkill>();
    }

    public void OnClick()
    {
        _slot?.SetSelectVFX(true);
        _upgradePanel?.Show(this, OnPanelClosed);
    }

    private void OnPanelClosed()
    {
        _slot?.SetSelectVFX(false);
    }

    public bool TryUpgrade()
    {
        int nextLevel = CurrentLevel + 1;
        var nextLevelData = CurrentData.getPokemonLevelDataByLevel(nextLevel);
        if (nextLevelData == null) return false; // đã max level

        int price = GetStat(CurrentData, nextLevel, "price");
        if (_playerStats.playerCoin < price) return false;

        _playerStats.MinusCoin(price);
        CurrentLevel = nextLevel;
        _skill?.SetLevel(CurrentLevel);
        return true;
    }

    public bool IsMaxLevel()
    {
        return !CurrentData.HasLevel(CurrentLevel + 1);
    }

    public bool TryEvolve()
    {
        if (!IsMaxLevel()) return false;
        if (CurrentData.EvolutionPokemonData == null) return false;

        int evoPrice = GetStat(CurrentData.EvolutionPokemonData, 1, "price");
        if (_playerStats.playerCoin < evoPrice) return false;

        _playerStats.MinusCoin(evoPrice);

        // Spawn mon mới tại cùng vị trí slot, slot sẽ clear mon cũ
        _slot.Evolve(CurrentData.EvolutionPokemonData);
        return true;
    }

    public void SellSelf()
    {
        int sellValue = GetStat(CurrentData, CurrentLevel, "sell");
        _playerStats.AddCoin(sellValue);
        _slot.Clear();
    }

    public int GetUpgradePrice()
    {
        if (IsMaxLevel()) return 0;
        return GetStat(CurrentData, CurrentLevel + 1, "price");
    }

    public int GetEvoPrice()
    {
        if (CurrentData.EvolutionPokemonData == null) return 0;
        return GetStat(CurrentData.EvolutionPokemonData, 1, "price");
    }

    public bool CanAffordUpgrade() => !IsMaxLevel() && _playerStats != null && _playerStats.playerCoin >= GetUpgradePrice();
    public bool CanAffordEvolve() => IsMaxLevel() && CurrentData.EvolutionPokemonData != null && _playerStats != null && _playerStats.playerCoin >= GetEvoPrice();
    public PlayerStatsInBattleManager GetPlayerStats() => _playerStats;

    public int GetSellValue() => GetStat(CurrentData, CurrentLevel, "sell");

    public double GetDamage() => GetStatDouble(CurrentData, CurrentLevel, "damage");
    public double GetCooldown() => GetStatDouble(CurrentData, CurrentLevel, "cd");

    private static int GetStat(PokemonData data, int level, string statName)
    {
        data.TryGetStatValueByLevel(level, statName, out double val);
        return (int)val;
    }

    private static double GetStatDouble(PokemonData data, int level, string statName)
    {
        data.TryGetStatValueByLevel(level, statName, out double val);
        return val;
    }
}
