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
		var bulletID = bulletClone.NetworkObjectId;
		UpdateClientBulletDataClientRpc(bulletID,genPos, angle, moveSec, maxDis);
	}
	//Todo using class to wrap all value
	[ClientRpc]
	private void UpdateClientBulletDataClientRpc(ulong bulletID ,Vector2 genPos, float angle, float moveSec, float maxDis){
		var bulletObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[bulletID];
		var bulletCtrl = bulletObj.GetComponent<BulletCtrl>();
		bulletCtrl.UpdateBulletData(genPos , angle , moveSec , maxDis);
	}
}