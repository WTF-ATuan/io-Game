using System;
using _Main_Tony._Test_PlayerCtrl.Runes;
using UniRx;
using UniRx.Triggers;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public class BulletObserver : NetworkBehaviour{
	public NetworkObject bulletNetworkObject;
	private IBattleCtrl _battleCtrl;

	[Inject]
	private void Initialization(IBattleCtrl battleCtrl, RunesMapper mapper){
		battleCtrl.SetSpawner(this);
		_battleCtrl = battleCtrl;
	}


	[ServerRpc(RequireOwnership = false)]
	public void SpawnBulletServerRpc(BulletData data){
		var bulletClone = Instantiate(bulletNetworkObject, data.genPos, Quaternion.Euler(0, 0, data.angle));
		bulletClone.Spawn();
		bulletClone.GetComponent<BulletCtrl>().Setup(data.genPos, data.angle, data.flySec, data.flyDis,
			() => { bulletClone.Despawn(); });
		bulletClone.OnCollisionEnterAsObservable().Subscribe(x => OnBulletHit(x, data.playerId));
	}

	//Server Only
	private void OnBulletHit(Collision obj, ulong playerId)
	{
		var parent = obj.transform.parent;
		if (parent == null) return;
		if(!parent.TryGetComponent<CreatureCtrl>(out var hitPlayer)) return;
		var hitPlayerId = hitPlayer.GetEntityID();
		if(playerId != hitPlayerId){
			_battleCtrl.PlayerHitRequestServerRpc(playerId, hitPlayerId, 35);
		}
	}
}

[Serializable]
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