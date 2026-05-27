using UnityEngine;
using System.Collections;

public class FloatAnimation : MonoBehaviour
{
    [Header("Float")]
    [Tooltip("Vertical distance in pixels the icon travels up/down from its origin")]
    public float amplitude = 12f;
    [Tooltip("Full cycles per second")]
    public float frequency = 0.8f;

    private RectTransform _rect;
    private Vector2 _originPos;
    private float _phase;
    private bool _active;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _originPos = _rect.anchoredPosition;
        _phase = Random.Range(0f, Mathf.PI * 2f);
    }

    private void OnEnable()
    {
        _active = false;
        float delay = Random.Range(0f, 1f);
        StartCoroutine(StartAfterDelay(delay));
    }

    private IEnumerator StartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _active = true;
    }

    private void Update()
    {
        if (!_active) return;
        float offsetY = Mathf.Sin(Time.time * frequency * Mathf.PI * 2f + _phase) * amplitude;
        _rect.anchoredPosition = _originPos + new Vector2(0f, offsetY);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _active = false;
        _rect.anchoredPosition = _originPos;
    }
}
