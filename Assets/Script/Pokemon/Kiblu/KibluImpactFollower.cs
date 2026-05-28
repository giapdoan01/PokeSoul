using UnityEngine;

public class KibluImpactFollower : MonoBehaviour
{
    private Transform followTarget;
    private float heightOffset;

    public void SetTarget(Transform target, float yOffset)
    {
        followTarget = target;
        heightOffset = yOffset;
    }

    private void Update()
    {
        if (followTarget == null || !followTarget.gameObject.activeInHierarchy)
        {
            followTarget = null;
            return;
        }

        transform.position = followTarget.position + Vector3.up * heightOffset;
    }

    private void OnDisable()
    {
        followTarget = null;
    }
}
