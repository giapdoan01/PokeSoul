using UnityEngine;
using System.Collections.Generic;

// Singleton đơn giản track danh sách enemy đang sống trong BattleScene.
// Thay thế FindGameObjectsWithTag — O(1) access thay vì O(n) scene scan.
public class EnemyRegistry : MonoBehaviour
{
    public static EnemyRegistry Instance { get; private set; }

    private readonly List<EnemyMoveController> _activeEnemies = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Register(EnemyMoveController enemy)
    {
        if (!_activeEnemies.Contains(enemy))
            _activeEnemies.Add(enemy);
    }

    public void Unregister(EnemyMoveController enemy)
    {
        _activeEnemies.Remove(enemy);
    }

    // Trả về enemy ưu tiên cao nhất trong range:
    // Ưu tiên 1: WaypointIndex lớn nhất (đi xa nhất trên path)
    // Ưu tiên 2: Gần waypoint tiếp theo nhất (sắp tới đích hơn)
    public bool TryGetPriorityTarget(Vector3 origin, float range, out Transform target)
    {
        target = null;
        float rangeSqr = range * range;
        int bestWaypointIndex = -1;
        float bestDistSqrToWaypoint = float.MaxValue;

        for (int i = _activeEnemies.Count - 1; i >= 0; i--)
        {
            var enemy = _activeEnemies[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                _activeEnemies.RemoveAt(i);
                continue;
            }

            float distSqr = (origin - enemy.transform.position).sqrMagnitude;
            if (distSqr > rangeSqr) continue;

            int waypointIndex = enemy.CurrentWayPointIndex;
            float distSqrToWaypoint = GetDistSqrToNextWaypoint(enemy, waypointIndex);

            bool isBetter = waypointIndex > bestWaypointIndex
                || (waypointIndex == bestWaypointIndex && distSqrToWaypoint < bestDistSqrToWaypoint);

            if (isBetter)
            {
                bestWaypointIndex = waypointIndex;
                bestDistSqrToWaypoint = distSqrToWaypoint;
                target = enemy.transform;
            }
        }

        return target != null;
    }

    // Lấy tất cả enemy trong range, sắp xếp theo priority (dùng cho multi-target skills)
    public List<Transform> GetTargetsInRange(Vector3 origin, float range, int maxCount)
    {
        float rangeSqr = range * range;
        var candidates = new List<(Transform t, int wp, float distSqr)>();

        for (int i = _activeEnemies.Count - 1; i >= 0; i--)
        {
            var enemy = _activeEnemies[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
            {
                _activeEnemies.RemoveAt(i);
                continue;
            }

            float distSqr = (origin - enemy.transform.position).sqrMagnitude;
            if (distSqr > rangeSqr) continue;

            candidates.Add((enemy.transform, enemy.CurrentWayPointIndex, GetDistSqrToNextWaypoint(enemy, enemy.CurrentWayPointIndex)));
        }

        candidates.Sort((a, b) =>
        {
            if (a.wp != b.wp) return b.wp.CompareTo(a.wp);
            return a.distSqr.CompareTo(b.distSqr);
        });

        var result = new List<Transform>(maxCount);
        for (int i = 0; i < Mathf.Min(maxCount, candidates.Count); i++)
            result.Add(candidates[i].t);

        return result;
    }

    private static float GetDistSqrToNextWaypoint(EnemyMoveController enemy, int waypointIndex)
    {
        if (enemy.wayPointManager == null || enemy.wayPointManager.wayPoints.Count == 0)
            return float.MaxValue;

        int idx = Mathf.Min(waypointIndex, enemy.wayPointManager.wayPoints.Count - 1);
        return (enemy.transform.position - enemy.wayPointManager.wayPoints[idx].position).sqrMagnitude;
    }
}
