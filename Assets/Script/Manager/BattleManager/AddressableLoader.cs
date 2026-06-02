using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressableLoader
{
    // Load 1 asset theo address, trả về asset + handle để release sau
    public static async Task<(T asset, AsyncOperationHandle<T> handle)> LoadAsync<T>(string address)
    {
        var handle = Addressables.LoadAssetAsync<T>(address);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AddressableLoader] Load failed: {address}");
            return (default, handle);
        }

        return (handle.Result, handle);
    }

    // Load 1 asset theo AssetReference
    public static async Task<(T asset, AsyncOperationHandle<T> handle)> LoadAsync<T>(AssetReference assetRef)
    {
        if (assetRef == null || !assetRef.RuntimeKeyIsValid())
        {
            Debug.LogError("[AddressableLoader] AssetReference is null or invalid.");
            return (default, default);
        }

        var handle = Addressables.LoadAssetAsync<T>(assetRef);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AddressableLoader] Load failed: {assetRef.RuntimeKey}");
            return (default, handle);
        }

        return (handle.Result, handle);
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
