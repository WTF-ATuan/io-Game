using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AdbLoader
{
    public static IDisposable Load<T>(string path, Action<T> onSussces, Action onFail = null)
    {
        var opHandle = Addressables.LoadAssetAsync<T>(path);
        opHandle.Completed += handle => {
            if (handle.Status==AsyncOperationStatus.Succeeded) {
                onSussces.Invoke(handle.Result);
            } else {
                onFail?.Invoke();
            }
        };
        return new DisAction(() => { Addressables.Release(opHandle); });
    }
    
    public static IDisposable LoadByLabel<T>(string labelToLoad, Action<IList<T>> onSussces, Action onFail = null)
    {
        var opHandle = Addressables.LoadAssetsAsync<T>(labelToLoad, null);
        Do();
        return new DisAction(() => { Addressables.Release(opHandle); });
        
        async void Do() {
            await opHandle.Task;
            if (opHandle.Status == AsyncOperationStatus.Succeeded) {
                IList<T> loadedAssets = opHandle.Result;
                onSussces.Invoke(loadedAssets);
            } else {
                onFail?.Invoke();
            }
        }
    }
}

public class DisAction:IDisposable
{
    private Action A;
    public DisAction(Action action) {
        A = action;
    }
    
    public void Dispose() {
        A.Invoke();
    }
}