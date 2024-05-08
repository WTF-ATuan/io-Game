using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public interface IBattleCtrl{
	public IDisposable AddCreature(CreatureCtrl player);
	public CreatureCtrl GetLocalPlayer();
	public ulong GetLocalPlayerID();
	public void SetSpawner(BulletObserver player);
	public BulletObserver GetSpawner();
	public void PlayerHitRequestServerRpc(ulong attackerId, ulong hitId, float damage);
	void AddedPlayerMoveForceRequestServerRpc(ulong targetId, Vector2 forceCenter);
	public List<CreatureCtrl> GetCreatureList();
	public List<GroundCtrl> GetGroundList();
	public bool AddPad(GroundCtrl ctrl);
	public bool RemovePad(GroundCtrl ctrl);
}

//Todo we can split Get Interface , Set Interface and Battle API Interface if IBattleCtrl is to large.
public class DemoBattleCtrl : IBattleCtrl{
	private BulletObserver _spawner;
	private readonly List<CreatureCtrl> _creatureList = new();
	public Dictionary<Vector2Int, GroundCtrl> _padsList = new();

	public IDisposable AddCreature(CreatureCtrl player){
		_creatureList.Add(player);
		return Disposable.Create(() => { _creatureList.Remove(player); });
	}

	public CreatureCtrl GetLocalPlayer(){
		var localClientId = NetworkManager.Singleton.LocalClientId;
		var playerCtrl = _creatureList.Find(x => x.OwnerClientId == localClientId);
		if(!playerCtrl) throw new NullReferenceException($"Can't find local player with{localClientId}");
		return playerCtrl;
	}

	public ulong GetLocalPlayerID(){
		var network = NetworkManager.Singleton;
		if(network.IsServer && !network.IsClient){
			return ulong.MaxValue; //Server Only ID
		}

		return GetLocalPlayer().GetEntityID();
	}

	public void SetSpawner(BulletObserver spawner){
		_spawner = spawner;
	}

	public BulletObserver GetSpawner(){
		return _spawner;
	}

	[ServerRpc(RequireOwnership = true)]
	public void PlayerHitRequestServerRpc(ulong attackerId, ulong hitId, float damage){
		var hitPlayer = GetCreatureList().Find(e => e.GetEntityID() == hitId);
		if(!hitPlayer || !hitPlayer.IsSpawned) return;
		var hitPlayerData = hitPlayer.GetSyncData().Value;
		Debug.Log($"{hitPlayerData.Health} {damage}");
		if(hitPlayerData.Health - damage <= 0){
			hitPlayer.DeathClientRpc();
			hitPlayer.NetworkObject.Despawn();
			ConditionLevel(attackerId, hitPlayerData);
		}
		else{
			hitPlayer.SetHealthClientRpc(hitPlayerData.Health - damage);
		}
	}

	private void ConditionLevel(ulong attackerId, AvaterState hitPlayerData){
		var attackPlayer = GetCreatureList().Find(e => e.GetEntityID() == attackerId);
		var attackPlayerData = attackPlayer.GetSyncData().Value;
		if(attackPlayerData.level >= hitPlayerData.level){
			(attackPlayer as PlayerCtrl)?.SetLevelClientRpc(attackPlayerData.level + 1);
		}
		else{
			(attackPlayer as PlayerCtrl)?.SetLevelClientRpc(hitPlayerData.level);
		}
	}

	[ServerRpc(RequireOwnership = true)]
	public void AddedPlayerMoveForceRequestServerRpc(ulong targetId, Vector2 forceCenter){
		var targetPlayer = GetCreatureList().Find(e => e.GetEntityID() == targetId);
		if(!targetPlayer || !targetPlayer.IsSpawned){
			return;
		}

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