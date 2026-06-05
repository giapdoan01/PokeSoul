using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressableLoader
{
    private const int MaxRetries = 3;
    private const float RetryDelaySeconds = 0.5f;

    // Load by string address with retry
    public static async Task<(T asset, AsyncOperationHandle<T> handle)> LoadAsync<T>(string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            Debug.LogError("[AddressableLoader] Address is null or empty.");
            return (default, default);
        }

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            var handle = Addressables.LoadAssetAsync<T>(address);

            // Use TaskCompletionSource to safely bridge Addressables callback to Task
            var tcs = new TaskCompletionSource<bool>();
            handle.Completed += op => tcs.TrySetResult(op.Status == AsyncOperationStatus.Succeeded);
            await tcs.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                return (handle.Result, handle);

            Debug.LogWarning($"[AddressableLoader] Attempt {attempt}/{MaxRetries} failed for: {address}");

            // Release the failed handle before retrying
            if (handle.IsValid())
                Addressables.Release(handle);

            if (attempt < MaxRetries)
                await Task.Delay((int)(RetryDelaySeconds * 1000 * attempt));
        }

        Debug.LogError($"[AddressableLoader] All {MaxRetries} attempts failed for: {address}");
        return (default, default);
    }

    // Load by AssetReference with retry
    public static async Task<(T asset, AsyncOperationHandle<T> handle)> LoadAsync<T>(AssetReference assetRef)
    {
        if (assetRef == null || !assetRef.RuntimeKeyIsValid())
        {
            Debug.LogError("[AddressableLoader] AssetReference is null or invalid.");
            return (default, default);
        }

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            var handle = Addressables.LoadAssetAsync<T>(assetRef);

            var tcs = new TaskCompletionSource<bool>();
            handle.Completed += op => tcs.TrySetResult(op.Status == AsyncOperationStatus.Succeeded);
            await tcs.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                return (handle.Result, handle);

            Debug.LogWarning($"[AddressableLoader] Attempt {attempt}/{MaxRetries} failed for: {assetRef.RuntimeKey}");

            if (handle.IsValid())
                Addressables.Release(handle);

            if (attempt < MaxRetries)
                await Task.Delay((int)(RetryDelaySeconds * 1000 * attempt));
        }

        Debug.LogError($"[AddressableLoader] All {MaxRetries} attempts failed for: {assetRef.RuntimeKey}");
        return (default, default);
    }

    // Release 1 handle
    public static void Release(AsyncOperationHandle handle)
    {
        if (handle.IsValid())
            Addressables.Release(handle);
    }

    // Release nhiều handles cùng lúc
    public static void ReleaseAll(List<AsyncOperationHandle> handles)
    {
        foreach (var handle in handles)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }
        handles.Clear();
    }
}
