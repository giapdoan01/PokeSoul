using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "PokeSoul/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string id;
    public string enemyName;
    public Sprite spriteEnemy;

    [Header("Prefab")]
    public AssetReference enemyPrefabRef;
}
