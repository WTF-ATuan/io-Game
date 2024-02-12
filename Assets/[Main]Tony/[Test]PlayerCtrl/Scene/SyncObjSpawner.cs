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
	public void SpawnBulletServerRpc(Vector2 genPos, float angle, float moveSec, float maxDis){
		var bulletClone = Instantiate(ButtetPrefab, genPos, Quaternion.Euler(0, 0, angle)).GetComponent<NetworkObject>();
		bulletClone.Spawn();
		bulletClone.GetComponent<BulletCtrl>().Setup(genPos,angle, moveSec, maxDis, () => { bulletClone.Despawn();});
	}

}