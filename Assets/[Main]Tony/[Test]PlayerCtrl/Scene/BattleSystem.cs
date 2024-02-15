using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public interface IBattleCtrl
{
    public void SetLocalPlayer(PlayerCtrl player);
    public PlayerCtrl GetLocalPlayer();
    
    public void SetSpawner(SyncObjSpawner player);
    public SyncObjSpawner GetSpawner();
    public void PlayerHitRequestServerRpc(ulong attackerId, ulong hitId, int damage);
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
    [ServerRpc(RequireOwnership = false)]
    public void PlayerHitRequestServerRpc(ulong attackerId, ulong hitId, int damage){
        Debug.Log($"Receive Hit Request {hitId} by {attackerId} ");
        var hitPlayer = NetworkManager.Singleton.ConnectedClients[hitId].PlayerObject;
        var playerCtrl = hitPlayer.GetComponent<PlayerCtrl>();
        playerCtrl.ModifyHealthClientRpc(-damage);
    }
}

public class BattleSystem : MonoInstaller {
    public override void InstallBindings() {
        Container.Bind<IBattleCtrl>().To<DemoBattleCtrl>().AsSingle().NonLazy();
    }
}
