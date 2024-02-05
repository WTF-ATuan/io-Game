using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public interface IServerCtrl
{
    public void ServerStart();
}

public class LocalServer : IServerCtrl
{
    public LocalServer()
    {
        WaitStart();
    }

    async Task WaitStart()
    {
        await UniTask.WaitUntil(()=>NetworkManager.Singleton != null);
        ServerStart();
    }

    public void ServerStart() {
        if (!NetworkManager.Singleton.IsServer) {
            NetworkManager.Singleton.StartHost(); //Host == Client + Server
        } else {
            NetworkManager.Singleton.StartClient();
        }
      
    }
}

public class ServerSystem : MonoInstaller {
    public override void InstallBindings() {
        Container.Bind<IServerCtrl>().To<LocalServer>().FromNew().AsSingle().NonLazy();
    }
}


