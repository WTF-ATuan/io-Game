using System;
using System.Collections.Generic;
using _Main_Tony._Test_PlayerCtrl.Runes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public interface IBattleCtrl{
	public void AddPlayer(PlayerCtrl player);
	public PlayerCtrl GetLocalPlayer();
	public ulong GetLocalPlayerID();
	public void SetSpawner(SyncObjSpawner player);
	public SyncObjSpawner GetSpawner();
	public void PlayerHitRequestServerRpc(ulong attackerId, ulong hitId, int damage);
	void RuneCastingRequestServerRpc(ulong playerId, ulong hitPlayerId, string runeId, RunesCastType runesCastType);
}

public class DemoBattleCtrl : IBattleCtrl{
	private SyncObjSpawner _spawner;
	private readonly List<PlayerCtrl> _playerList = new();

	public void AddPlayer(PlayerCtrl player){
		_playerList.Add(player);
	}

	public PlayerCtrl GetLocalPlayer(){
		var localClientId = NetworkManager.Singleton.LocalClientId;
		var playerCtrl = _playerList.Find(x => x.OwnerClientId == localClientId);
		if(!playerCtrl) throw new NullReferenceException($"Can't find local player with{localClientId}");
		return playerCtrl;
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

	[ServerRpc(RequireOwnership = false)]
	public void RuneCastingRequestServerRpc(ulong playerId, ulong hitPlayerId, string runesId,
		RunesCastType runesCastType){
		Debug.Log($"{playerId} cast {runesId} to {hitPlayerId} with {runesCastType} ");
	}
}

public class BattleSystem : MonoInstaller{
	public override void InstallBindings(){
		Container.Bind<IBattleCtrl>().To<DemoBattleCtrl>().AsSingle().NonLazy();
	}
}