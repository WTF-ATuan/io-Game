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

	public GameObject ButtetPrefab;

	[ServerRpc(RequireOwnership = false)]
	public void SpawnBulletServerRpc(BulletData data){
		var bulletClone = Instantiate(ButtetPrefab, data.genPos, Quaternion.Euler(0, 0, data.angle))
				.GetComponent<NetworkObject>();
		bulletClone.Spawn();
		bulletClone.GetComponent<BulletCtrl>().Setup(data.genPos, data.angle, data.moveSec, data.maxDis,
			() => { bulletClone.Despawn(); });
		bulletClone.OnCollisionEnterAsObservable().Subscribe(x => OnBulletHit(x, data.playerId));
		GenerateBulletViewClientRpc(bulletClone.NetworkObjectId, data.runesId);
	}

	[ClientRpc]
	private void GenerateBulletViewClientRpc(ulong bulletId, string runesId){
		var spawnedObject = NetworkManager.SpawnManager.SpawnedObjects[bulletId];
		var viewObject = _mapper.GetViewObjectById(runesId);
		Instantiate(viewObject, spawnedObject.transform.position, Quaternion.identity, spawnedObject.transform);
	}

	//Server Only
	private void OnBulletHit(Collision obj, ulong playerId){
		if(!obj.transform.parent.TryGetComponent<PlayerCtrl>(out var hitPlayer)){
			return;
		}

		var hitPlayerId = hitPlayer.OwnerClientId;
		if(playerId != hitPlayerId){
			_battleCtrl.PlayerHitRequestServerRpc(playerId, hitPlayerId, 100);
		}
	}
}

[System.Serializable]
public class BulletData : INetworkSerializable{
	public Vector2 genPos;
	public float angle;
	public float moveSec;
	public float maxDis;
	public ulong playerId;
	public string runesId;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter{
		serializer.SerializeValue(ref genPos);
		serializer.SerializeValue(ref angle);
		serializer.SerializeValue(ref moveSec);
		serializer.SerializeValue(ref maxDis);
		serializer.SerializeValue(ref playerId);
		serializer.SerializeValue(ref runesId);
	}
}