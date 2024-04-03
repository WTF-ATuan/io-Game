using System;
using System.Collections.Generic;
using System.Linq;
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
	void AddedPlayerMoveForceRequestServerRpc(ulong targetId, Vector2 forceCenter);
	public List<CreatureCtrl> GetCreatureList();
	public List<GroundCtrl> GetGroundList();
	public bool AddPad(GroundCtrl ctrl);
	public bool RemovePad(GroundCtrl ctrl);
}

//Todo we can split Get Interface , Set Interface and Battle API Interface if IBattleCtrl is to large.
public class DemoBattleCtrl : IBattleCtrl{
	private SyncObjSpawner _spawner;
	private readonly List<CreatureCtrl> _creatureList = new();
	public Dictionary<Vector2Int, GroundCtrl> _padsList = new();

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
		var network = NetworkManager.Singleton;
		if(network.IsServer && !network.IsClient){
			return ulong.MaxValue; //Server Only ID
		}

		return network.LocalClientId;
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
		var avaterState = playerCtrl.GetSyncData().Value;
		var avaterMaxHealth = playerCtrl.GetLoadOut().NowAttribute.MaxHealth;
		if(avaterState.Health - damage <= 0){
			playerCtrl.DeathClientRpc();
			hitPlayer.Despawn();
		}
		else{
			var newAvaterHealth = Mathf.Clamp(avaterState.Health - damage, 0, avaterMaxHealth);
			playerCtrl.SetHealthClientRpc(newAvaterHealth);
		}
	}
	[ServerRpc(RequireOwnership = false)]
	public void AddedPlayerMoveForceRequestServerRpc(ulong targetId, Vector2 forceCenter){
		var targetPlayer = NetworkManager.Singleton.ConnectedClients[targetId].PlayerObject;
		var playerCtrl = targetPlayer.GetComponent<PlayerCtrl>();
		playerCtrl.ForceToClientRpc(forceCenter);
	}

	public List<CreatureCtrl> GetCreatureList(){
		return new List<CreatureCtrl>(_creatureList);
	}

	public List<GroundCtrl> GetGroundList(){
		return _padsList.Values.ToList();
	}

	public bool AddPad(GroundCtrl ctrl){
		if(_padsList.ContainsKey(ctrl.GetPos())) return false;
		_padsList.Add(ctrl.GetPos(), ctrl);
		return true;
	}

	public bool RemovePad(GroundCtrl ctrl){
		if(!_padsList.ContainsKey(ctrl.GetPos())) return false;
		_padsList.Remove(ctrl.GetPos());
		return true;
	}
}

public class BattleSystem : MonoInstaller{
	public override void InstallBindings(){
		Container.Bind<IBattleCtrl>().To<DemoBattleCtrl>().AsSingle().NonLazy();
	}
}