using UniRx;
using UniRx.Triggers;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public class SyncObjSpawner : NetworkBehaviour{
	[Inject]
	private void Initialization(IBattleCtrl battleCtrl){
		battleCtrl.SetSpawner(this);
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
		if(!obj.transform.root.TryGetComponent<PlayerCtrl>(out var hitPlayer)){
			return;
		}

		var hitPlayerId = hitPlayer.GetComponent<NetworkObject>().OwnerClientId;
		if(playerId != hitPlayerId){
			PlayerHitRequestServerRpc(playerId, hitPlayerId, 5);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	public void PlayerHitRequestServerRpc(ulong attackerId, ulong hitId, int damage){
		Debug.Log($"Receive Hit Request {hitId} by {attackerId} ");
	}
}