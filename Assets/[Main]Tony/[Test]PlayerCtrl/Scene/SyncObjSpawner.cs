using UniRx;
using UniRx.Triggers;
using Unity.Netcode;
using UnityEngine;
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
	public void SpawnBulletServerRpc(Vector2 genPos, float angle, float moveSec, float maxDis,
		ulong playerId = default){
		var bulletClone = Instantiate(ButtetPrefab, genPos, Quaternion.Euler(0, 0, angle))
				.GetComponent<NetworkObject>();
		bulletClone.Spawn();
		bulletClone.GetComponent<BulletCtrl>().Setup(genPos, angle, moveSec, maxDis, () => { bulletClone.Despawn(); });
		bulletClone.OnCollisionEnterAsObservable().Subscribe(x => OnBulletHit(x, playerId));
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