using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IBattleCtrl
{
    public void SetLocalPlayer(PlayerCtrl player);
    public PlayerCtrl GetLocalPlayer();
}

public class DemoBattleCtrl : IBattleCtrl
{
    private PlayerCtrl LocalPlayer;

    public void SetLocalPlayer(PlayerCtrl player) {
        LocalPlayer = player;
    }
    
    public PlayerCtrl GetLocalPlayer() {
        return LocalPlayer;
    }
}

public class BattleSystem : MonoInstaller {
    public override void InstallBindings() {
        Container.Bind<IBattleCtrl>().To<DemoBattleCtrl>().AsSingle().NonLazy();
    }
}
