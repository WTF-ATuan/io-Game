using UniRx;
using UniRx.Triggers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class SyncObjSpawner : NetworkBehaviour{
	private IBattleCtrl _battleCtrl;

	[Inject]
	private void Initialization(IBattleCtrl battleCtrl){
		battleCtrl.SetSpawner(this);
		_battleCtrl = battleCtrl;
	}

	public GameObject ButtetPrefab;

	[ServerRpc(RequireOwnership = false)]
	public void SpawnBulletServerRpc(BulletData data){
		var bulletClone = Instantiate(ButtetPrefab, data.genPos, Quaternion.Euler(0, 0, data.angle))
				.GetComponent<NetworkObject>();
		bulletClone.Spawn();
		bulletClone.GetComponent<BulletCtrl>().Setup(data.genPos, data.angle, data.moveSec, data.maxDis, () => { bulletClone.Despawn(); });
		bulletClone.OnCollisionEnterAsObservable().Subscribe(x => OnBulletHit(x, data.playerId));
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
	
	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter{
		serializer.SerializeValue(ref genPos);
		serializer.SerializeValue(ref angle);
		serializer.SerializeValue(ref moveSec);
		serializer.SerializeValue(ref maxDis);
		serializer.SerializeValue(ref playerId);
	}
}