using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IBattleCtrl
{
    public void SetLocalPlayer(PlayerCtrl player);
    public PlayerCtrl GetLocalPlayer();
    
    public void SetSpawner(SyncObjSpawner player);
    public SyncObjSpawner GetSpawner();
}

public class DemoBattleCtrl : IBattleCtrl
{
    private PlayerCtrl LocalPlayer;
    private SyncObjSpawner Spawner;

    public void SetLocalPlayer(PlayerCtrl player) {
        LocalPlayer = player;
    }
    
    public PlayerCtrl GetLocalPlayer() {
        return LocalPlayer;
    }

    public void SetSpawner(SyncObjSpawner spawner)
    {
        Spawner = spawner;
    }

    public SyncObjSpawner GetSpawner()
    {
        return Spawner;
    }
}

public class BattleSystem : MonoInstaller {
    public override void InstallBindings() {
        Container.Bind<IBattleCtrl>().To<DemoBattleCtrl>().AsSingle().NonLazy();
    }
}
