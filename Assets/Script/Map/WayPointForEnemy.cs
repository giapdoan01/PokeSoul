using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WayPointForEnemy : MonoBehaviour
{
    public List<Transform> wayPoints;
    public void getWayPoint(int index, out Transform wayPoint)
    {
        if (index < wayPoints.Count)
        {
            wayPoint = wayPoints[index];
        }
        else
        {
            Debug.LogWarning($"[WayPointForEnemy] Không tìm thấy WayPoint với index: {index}");
            wayPoint = null;
        }
    }

    
}