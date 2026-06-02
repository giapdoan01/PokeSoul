using UnityEngine;

public class VerdrakeParticleController : MonoBehaviour
{
    [SerializeField] private Transform ball;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float arriveRadius = 0.2f;

    private bool _arrived;
    private GameObject _impactPrefab;
    private float _impactLifeTime;
    private Vector3 _ballStartLocalPos;

    private const string ImpactPoolKey = "VerdrakeImpact";

    private void Awake()
    {
        if (ball == null)
            ball = transform.Find("Ball");

        if (ball != null)
            _ballStartLocalPos = ball.localPosition;
    }

    public void Initialize(GameObject impactPrefab, float impactLifeTime)
    {
        _impactPrefab = impactPrefab;
        _impactLifeTime = impactLifeTime;
        _arrived = false;

        if (ball != null)
        {
            ball.localPosition = _ballStartLocalPos;
            ball.gameObject.SetActive(true);
        }
    }

    private void OnEnable()
    {
        _arrived = false;
    }

    private void Update()
    {
        if (_arrived || ball == null || !ball.gameObject.activeSelf) return;

        Vector3 direction = transform.position - ball.position;

        if (direction.sqrMagnitude > 0.0001f)
        {
            ball.position += direction.normalized * moveSpeed * Time.deltaTime;
            ball.forward = direction.normalized;
        }

        if (Vector3.Distance(ball.position, transform.position) <= arriveRadius)
            Arrive();
    }

    private void Arrive()
    {
        _arrived = true;
        SpawnImpact();
        ball.gameObject.SetActive(false);
        GetComponent<VerdrakeSkill>()?.OnParticleArrived();
    }

    private void SpawnImpact()
    {
        if (_impactPrefab == null) return;

        if (SkillObjectPolling.Instance != null)
        {
            GameObject impact = SkillObjectPolling.Instance.GetFromPool(ImpactPoolKey, transform.position, Quaternion.identity);
            if (impact != null)
            {
                SkillPoolToken token = impact.GetComponent<SkillPoolToken>();
                if (token != null)
                {
                    token.ReturnToPoolAfterDelay(_impactLifeTime);
                    return;
                }
            }
        }

        GameObject fallback = Instantiate(_impactPrefab, transform.position, Quaternion.identity);
        if (_impactLifeTime > 0f)
            Destroy(fallback, _impactLifeTime);
    }
}
