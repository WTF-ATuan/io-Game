using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public interface IBattleCtrl{
	public void AddCreature(CreatureCtrl player);
	public PlayerCtrl GetLocalPlayer();
	public ulong GetLocalPlayerID();
	public void SetSpawner(SyncObjSpawner player);
	public SyncObjSpawner GetSpawner();
	public void PlayerHitRequestServerRpc(ulong attackerId, ulong hitId, int damage);

	public List<CreatureCtrl> GetCreatureList();
}
//Todo we can split Get Interface , Set Interface and Battle API Interface if IBattleCtrl is to large.
public class DemoBattleCtrl : IBattleCtrl{
	private SyncObjSpawner _spawner;
	private readonly List<CreatureCtrl> _creatureList = new();

	public void AddCreature(CreatureCtrl player){
		_creatureList.Add(player);
	}

	public PlayerCtrl GetLocalPlayer(){
		var localClientId = NetworkManager.Singleton.LocalClientId;
		var playerCtrl = _creatureList.Find(x => x.OwnerClientId == localClientId);
		if(!playerCtrl) throw new NullReferenceException($"Can't find local player with{localClientId}");
		return (PlayerCtrl)playerCtrl;
	}

	public ulong GetLocalPlayerID(){
		return NetworkManager.Singleton.LocalClientId;
	}

	public void SetSpawner(SyncObjSpawner spawner){
		_spawner = spawner;
	}

	public SyncObjSpawner GetSpawner(){
		return _spawner;
	}

	[ServerRpc(RequireOwnership = false)]
	public void PlayerHitRequestServerRpc(ulong attackerId, ulong hitId, int damage){
		var hitPlayer = NetworkManager.Singleton.ConnectedClients[hitId].PlayerObject;
		var playerCtrl = hitPlayer.GetComponent<PlayerCtrl>();
		playerCtrl.ModifyHealthClientRpc(-damage);
	}

	public List<CreatureCtrl> GetCreatureList() {
		return new List<CreatureCtrl>(_creatureList);
	}
}

public class BattleSystem : MonoInstaller{
	public override void InstallBindings(){
		Container.Bind<IBattleCtrl>().To<DemoBattleCtrl>().AsSingle().NonLazy();
	}
}