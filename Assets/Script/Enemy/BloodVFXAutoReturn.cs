using System.Collections;
using UnityEngine;

// Gắn vào BloodVFX prefab — tự return về EnemyObjectPool sau lifeTime giây.
public class BloodVFXAutoReturn : MonoBehaviour
{
    public float lifeTime = 1.5f;

    private void OnEnable()
    {
        StartCoroutine(ReturnAfterDelay());
    }

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(lifeTime);
        EnemyObjectPool.Instance?.Return(gameObject);
    }
}
