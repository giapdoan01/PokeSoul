using UnityEngine;
using System.Collections.Generic;

public class GekigumaPaw : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] private GameObject impactVFX;
    [SerializeField] private GameObject releaseVFX;
    [SerializeField] private float releaseVFXHeight = 0.1f;
    [SerializeField] private float vfxLifeTime = 1.5f;

    private const string ImpactPoolKey   = "GekigumaImpact";
    private const string ReleasePoolKey  = "GekigumaRelease";

    private AudioSource _audioSource;
    private GekigumaPaw _otherPaw;
    private double _damage;
    private float _preCastDelay;
    private float _moveSpeed;
    private AudioClip _impactSFX;
    private AudioClip _loopSFX;
    private float _timer;
    private bool _isFlying;
    private bool _didMeet;
    private bool _playLoopSFX;
    private readonly HashSet<EnemyHPController> _hitEnemies = new();

    private Collider[] _colliders;
    private Rigidbody _rb;

    // ── Init ──

    private void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>(true);
        _rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
        SetCollidersActive(false);
        RegisterPools();
    }

    private void RegisterPools()
    {
        if (SkillObjectPolling.Instance == null) return;
        if (impactVFX   != null) SkillObjectPolling.Instance.RegisterPrefab(ImpactPoolKey,   impactVFX,   3);
        if (releaseVFX  != null) SkillObjectPolling.Instance.RegisterPrefab(ReleasePoolKey,  releaseVFX,  1);
    }

    public void Launch(GekigumaPaw otherPaw, double damage, float preCastDelay, float moveSpeed, AudioClip impactSFX, AudioClip loopSFX = null, bool playLoop = false)
    {
        _otherPaw     = otherPaw;
        _damage       = damage;
        _preCastDelay = preCastDelay;
        _moveSpeed    = moveSpeed;
        _impactSFX    = impactSFX;
        _loopSFX      = loopSFX;
        _playLoopSFX  = playLoop;
        _timer        = 0f;
        _isFlying     = false;
        _didMeet      = false;
        _hitEnemies.Clear();

        SetCollidersActive(false);
        gameObject.SetActive(true);
    }

    // ── Update ──

    private void Update()
    {
        if (_didMeet) return;
        _timer += Time.deltaTime;

        if (!_isFlying)
        {
            if (_timer >= _preCastDelay)
            {
                _isFlying = true;
                SetCollidersActive(true);

                if (_playLoopSFX && _loopSFX != null && _audioSource != null)
                {
                    _audioSource.clip = _loopSFX;
                    _audioSource.loop = true;
                    _audioSource.Play();
                }
            }
            return;
        }

        // Bay về vị trí hiện tại của paw kia
        if (_otherPaw != null && _otherPaw.gameObject.activeInHierarchy)
        {
            Vector3 targetPos = _otherPaw.transform.position;
            Vector3 dir = targetPos - transform.position;

            if (dir.sqrMagnitude < 0.09f) // 0.3 * 0.3 → gặp nhau
            {
                OnMeet();
                return;
            }

            transform.forward = dir.normalized;
            transform.position += dir.normalized * _moveSpeed * Time.deltaTime;
        }
        else
        {
            // Paw kia đã biến mất → release
            OnMeet();
        }
    }

    // ── Hit ──

    private void OnTriggerEnter(Collider other)
    {
        if (!_isFlying || _didMeet) return;
        var hp = other.GetComponentInParent<EnemyHPController>() ?? other.GetComponent<EnemyHPController>();
        if (hp == null || _hitEnemies.Contains(hp)) return;

        _hitEnemies.Add(hp);
        hp.TakeDamage(_damage);

        SpawnVFX(ImpactPoolKey, impactVFX, hp.transform.position + Vector3.up * 0.5f);
        MonImpactSoundManager.Instance?.PlaySound(_impactSFX);
    }

    // ── Meet ──

    private void OnMeet()
    {
        _didMeet = true;

        if (_audioSource != null) _audioSource.Stop();

        SpawnVFX(ReleasePoolKey, releaseVFX, transform.position + Vector3.up * releaseVFXHeight);
        Release();
    }

    // ── Helpers ──

    private void SetCollidersActive(bool active)
    {
        if (_colliders == null) _colliders = GetComponentsInChildren<Collider>(true);
        foreach (var col in _colliders)
        {
            col.isTrigger = true;
            col.enabled = active;
        }

        if (_rb == null) _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }
    }

    private void SpawnVFX(string poolKey, GameObject fallbackPrefab, Vector3 position)
    {
        if (fallbackPrefab == null) return;
        RegisterPools();

        GameObject go = SkillObjectPolling.Instance != null
            ? SkillObjectPolling.Instance.GetFromPool(poolKey, position, Quaternion.identity)
            : Instantiate(fallbackPrefab, position, Quaternion.identity);

        if (go != null)
        {
            var token = go.GetComponent<SkillPoolToken>();
            if (token != null) token.ReturnToPoolAfterDelay(vfxLifeTime);
        }
    }

    private void Release()
    {
        if (SkillObjectPolling.Instance != null)
            SkillObjectPolling.Instance.ReturnByInstance(gameObject);
        else
            Destroy(gameObject);
    }

    private void OnDisable()
    {
        _isFlying  = false;
        _didMeet   = false;
        _otherPaw  = null;
        _hitEnemies.Clear();
        _audioSource?.Stop();
    }
}
