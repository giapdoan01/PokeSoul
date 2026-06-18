using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class EnemyStatusEffects : MonoBehaviour
{
    public bool IsImmobilized { get; private set; }
    public bool IsEnchanted { get; private set; }
    public float SpeedMultiplier { get; private set; } = 1f;

    private CancellationTokenSource _stunCts;
    private CancellationTokenSource _enchantCts;

    public void Stun(float duration)
    {
        _stunCts?.Cancel();
        _stunCts = new CancellationTokenSource();
        StunAsync(duration, _stunCts.Token).Forget();
    }

    public void Enchant(float duration)
    {
        _enchantCts?.Cancel();
        _enchantCts = new CancellationTokenSource();
        EnchantAsync(duration, _enchantCts.Token).Forget();
    }

    private async UniTaskVoid StunAsync(float duration, CancellationToken ct)
    {
        IsImmobilized = true;
        SpeedMultiplier = 0f;
        await UniTask.Delay((int)(duration * 1000), cancellationToken: ct);
        IsImmobilized = false;
        SpeedMultiplier = 1f;
    }

    private async UniTaskVoid EnchantAsync(float duration, CancellationToken ct)
    {
        IsEnchanted = true;
        await UniTask.Delay((int)(duration * 1000), cancellationToken: ct);
        IsEnchanted = false;
    }

    private void OnDestroy()
    {
        _stunCts?.Cancel();
        _enchantCts?.Cancel();
    }
}
