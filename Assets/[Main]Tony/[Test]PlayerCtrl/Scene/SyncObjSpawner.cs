using System;
using _Main_Tony._Test_PlayerCtrl.Runes;
using UniRx;
using UniRx.Triggers;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public class SyncObjSpawner : NetworkBehaviour{
	private IBattleCtrl _battleCtrl;
	private RunesMapper _mapper;

	[Inject]
	private void Initialization(IBattleCtrl battleCtrl, RunesMapper mapper){
		battleCtrl.SetSpawner(this);
		_battleCtrl = battleCtrl;
		_mapper = mapper;
	}
	public override void OnNetworkSpawn(){
		/*if(IsServer) {
			NetworkManager.Singleton.OnClientConnectedCallback += a;
		}*/
	}

	private void a(ulong clientID){
		/*SpawnMobServerRpc();
		NetworkManager.Singleton.OnClientConnectedCallback -= a;*/
	}

	public GameObject MobPrefab;
	[ServerRpc(RequireOwnership = false)]
	public void SpawnMobServerRpc(Vector2 spawnPos){
		var entity = Instantiate(ButtetPrefab, spawnPos, Quaternion.identity).GetComponent<NetworkObject>();
		entity.Spawn();
	}
	
	public GameObject ButtetPrefab;

	[ServerRpc(RequireOwnership = false)]
	public void SpawnBulletServerRpc(BulletData data){
		var bulletClone = Instantiate(ButtetPrefab, data.genPos, Quaternion.Euler(0, 0, data.angle))
				.GetComponent<NetworkObject>();
		bulletClone.Spawn();
		bulletClone.GetComponent<BulletCtrl>().Setup(data.genPos, data.angle, data.flySec, data.flyDis,
			() => { bulletClone.Despawn(); });
		bulletClone.OnCollisionEnterAsObservable().Subscribe(x => OnBulletHit(x, data.playerId));
		GenerateBulletViewClientRpc(bulletClone.NetworkObjectId, data.runesId);
	}

	[ClientRpc]
	private void GenerateBulletViewClientRpc(ulong bulletId, string runesId){
		if(runesId != "Frozen") return;
		var spawnedObject = NetworkManager.SpawnManager.SpawnedObjects[bulletId];
		var viewObject = _mapper.GetViewObjectById(runesId);
		Instantiate(viewObject, spawnedObject.transform.position, Quaternion.identity, spawnedObject.transform);
	}

	//Server Only
	private void OnBulletHit(Collision obj, ulong playerId)
	{
		var parent = obj.transform.parent;
		if (parent == null) return;
		if(!parent.TryGetComponent<CreatureCtrl>(out var hitPlayer)) return;
		var hitPlayerId = hitPlayer.GetEntityID();
		if(playerId != hitPlayerId){
			_battleCtrl.PlayerHitRequestServerRpc(playerId, hitPlayerId, 100);
		}
	}
}

[System.Serializable]
public class BulletData : INetworkSerializable{
	public Vector2 genPos;
	public float angle;
	public float flySec;
	public float flyDis;
	public float damage;
	public ulong playerId;
	public string runesId;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter{
		serializer.SerializeValue(ref genPos);
		serializer.SerializeValue(ref angle);
		serializer.SerializeValue(ref flySec);
		serializer.SerializeValue(ref flyDis);
		serializer.SerializeValue(ref playerId);
		serializer.SerializeValue(ref runesId);
		serializer.SerializeValue(ref damage);
	}
}