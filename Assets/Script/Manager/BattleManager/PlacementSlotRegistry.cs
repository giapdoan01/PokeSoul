using UnityEngine;
using System.Collections.Generic;

// Static registry — PlacementSlot tự đăng ký/hủy khi Enable/Disable.
// Không cần FindObjectOfType, không tốn overhead scene scan.
public static class PlacementSlotRegistry
{
    private static readonly List<PlacementSlot> _slots = new();

    public static void Register(PlacementSlot slot)   => _slots.Add(slot);
    public static void Unregister(PlacementSlot slot) => _slots.Remove(slot);

    // Tìm slot trống gần nhất với screenPos (world-space raycast từ camera)
    public static PlacementSlot FindNearestFree(Vector3 worldPos, float maxDistance = 5f)
    {
        PlacementSlot best = null;
        float bestDist = maxDistance;

        foreach (var slot in _slots)
        {
            if (slot.IsOccupied) continue;
            float d = Vector3.Distance(slot.transform.position, worldPos);
            if (d < bestDist) { bestDist = d; best = slot; }
        }

        return best;
    }
}
