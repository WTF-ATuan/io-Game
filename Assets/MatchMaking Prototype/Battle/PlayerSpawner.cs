using System;
using Unity.Netcode;
using UnityEngine;

namespace MatchMaking_Prototype.Battle{
	public class PlayerSpawner : NetworkBehaviour{
		[SerializeField] private NetworkObject playerPrefab;

		public override void OnNetworkSpawn()
		{
			if (!IsServer) { return; }
			
			foreach(var client in NetworkManager.Singleton.ConnectedClients){
					var spawnPos = Vector3.zero;
					var characterInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
					characterInstance.SpawnAsPlayerObject(client.Key);
			}
		}

		public void TestCreate(){
			Instantiate(playerPrefab.gameObject, Vector3.zero, Quaternion.identity);
		}

	}
}