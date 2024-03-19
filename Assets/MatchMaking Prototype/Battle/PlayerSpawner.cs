using Unity.Netcode;
using UnityEngine;

namespace MatchMaking_Prototype.Battle{
	public class PlayerSpawner : NetworkBehaviour{
		[SerializeField] private NetworkObject playerPrefab;
		
		public override void OnNetworkSpawn()
		{
			if (!IsServer) { return; }

			foreach (var client in MatchplayNetworkServer.Instance.ClientData)
			{
				var spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
				var characterInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
				characterInstance.SpawnAsPlayerObject(client.Value.clientId);
			}
		}
		
	}
}