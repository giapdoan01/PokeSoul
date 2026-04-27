using UnityEngine;

public interface IPokemonSkillLaunch
{
    void Launch(PokemonSkill owner, Transform targetEnemy, PokemonData pokemonData, int level, string attack);
}
