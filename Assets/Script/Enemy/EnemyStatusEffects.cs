using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyStatusEffects : MonoBehaviour
{
    public float SpeedMultiplier { get; private set; } = 1f;
    public bool IsImmobilized { get; private set; } = false;

    private CancellationTokenSource _slowCts;
    private CancellationTokenSource _freezeCts;
    private CancellationTokenSource _stunCts;

    // ── PUBLIC API ────────────────────────────────────────────────

    /// <summary>Làm chậm enemy. multiplier: 0.0 - 1.0 (vd: 0.5 = chậm 50%)</summary>
    public void ApplySlow(float multiplier, float duration)
    {
        _slowCts?.Cancel();
        _slowCts = new CancellationTokenSource();
        SlowAsync(multiplier, duration, _slowCts.Token).Forget();
    }

    /// <summary>Bất động hoàn toàn (SpeedMultiplier = 0)</summary>
    public void ApplyFreeze(float duration)
    {
        _freezeCts?.Cancel();
        _freezeCts = new CancellationTokenSource();
        FreezeAsync(duration, _freezeCts.Token).Forget();
    }

    /// <summary>Stun: bất động + speed = 0</summary>
    public void ApplyStun(float duration)
    {
        _stunCts?.Cancel();
        _stunCts = new CancellationTokenSource();
        StunAsync(duration, _stunCts.Token).Forget();
    }

    public void ClearAll()
    {
        _slowCts?.Cancel();
        _freezeCts?.Cancel();
        _stunCts?.Cancel();
        SpeedMultiplier = 1f;
        IsImmobilized = false;
    }

    private void OnDestroy() => ClearAll();

    // ── ASYNC ─────────────────────────────────────────────────────

    private async UniTaskVoid SlowAsync(float multiplier, float duration, CancellationToken ct)
    {
        SpeedMultiplier = Mathf.Clamp(multiplier, 0.05f, 1f);
        await UniTask.Delay((int)(duration * 1000), cancellationToken: ct);
        SpeedMultiplier = 1f;
    }

    private async UniTaskVoid FreezeAsync(float duration, CancellationToken ct)
    {
        IsImmobilized = true;
        await UniTask.Delay((int)(duration * 1000), cancellationToken: ct);
        IsImmobilized = false;
    }

    private async UniTaskVoid StunAsync(float duration, CancellationToken ct)
    {
        IsImmobilized = true;
        SpeedMultiplier = 0f;
        await UniTask.Delay((int)(duration * 1000), cancellationToken: ct);
        IsImmobilized = false;
        SpeedMultiplier = 1f;
    }
}
