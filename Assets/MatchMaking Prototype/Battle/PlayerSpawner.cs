using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MatchMaking_Prototype.Battle{
	public class PlayerSpawner : NetworkBehaviour{
		[SerializeField] private NetworkObject playerPrefab;
		[SerializeField] private NetworkObject mobPrefab;

		public override void OnNetworkSpawn(){
			if(!IsServer){
				return;
			}

			var battleLevelInfo = MatchplayNetworkServer.Instance.GetBattleLevelInfo();
			if(battleLevelInfo.battleType == BattleType.Pve){
				Instantiate(mobPrefab, Vector3.zero, quaternion.identity);
			}

			foreach(var client in MatchplayNetworkServer.Instance.ClientData){
				var spawnPos = new Vector2(Random.Range(0, 1), Random.Range(0, 1));
				var characterInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
				characterInstance.SpawnAsPlayerObject(client.Value.clientId);
			}
		}
	}
}