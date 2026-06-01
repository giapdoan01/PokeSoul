using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AllMap", menuName = "PokeSoul/All Map")]
public class AllMap : ScriptableObject
{
    public List<Map> maps;

    public Map GetMapById(string id)
    {
        foreach (var map in maps)
        {
            if (map.id == id)
                return map;
        }
        Debug.LogWarning($"[AllMap] Không tìm thấy map với id: {id}");
        return null;
    }
}
