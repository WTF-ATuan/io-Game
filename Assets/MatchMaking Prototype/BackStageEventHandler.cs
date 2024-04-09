using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scenes{
	public class BackStageEventHandler : NetworkBehaviour{
		[SerializeField] private float readyTime = 30;

		[Header("Local Component")] [SerializeField]
		private Transform timeBarMask;

		[SerializeField] private Button readyButton;


		private float _timeCounter;
		private bool _isReady;
		private NetworkList<PlayerLobbyState> _playerStatesList;

		private void Awake(){
			_playerStatesList = new NetworkList<PlayerLobbyState>();
			readyButton.onClick.AddListener(Ready);
		}

		public override void OnNetworkSpawn(){
			if(!IsServer) return;
			foreach(var connectedClient in NetworkManager.Singleton.ConnectedClients){
				_playerStatesList.Add(new PlayerLobbyState(connectedClient.Key, ulong.MinValue, false));
			}
		}

		public void FixedUpdate(){
			if(IsServer){
				ServerTimeCounter();
			}
		}

		private void ServerTimeCounter(){
			if(_isReady) return;

			if(_timeCounter > readyTime){
				Ready();
			}

			_timeCounter += NetworkManager.Singleton.ServerTime.FixedDeltaTime;
			ServerTimeUpdatedClientRpc(_timeCounter);
		}

		private void Ready(){
			ReadyRequestServerRpc();
		}

		[ServerRpc(RequireOwnership = false)]
		private void ReadyRequestServerRpc(ServerRpcParams serverRpcParams = default){
			for(var index = 0; index < _playerStatesList.Count; index++){
				var state = _playerStatesList[index];
				if(serverRpcParams.Receive.SenderClientId != state.ClientID) continue;
				_playerStatesList[index] = new PlayerLobbyState(state.ClientID, state.ViewID, true);
				_isReady = true;
				ReadyStateChangedClientRpc(state.ClientID);
			}

			foreach(var player in _playerStatesList){
				if(!player.IsReady){
					return;
				}
			}

			MatchplayNetworkServer.Instance.StartBattle();
		}


		[ClientRpc]
		private void ReadyStateChangedClientRpc(ulong clientId){
			foreach(var state in _playerStatesList){
				if(clientId != state.ClientID) continue;
				readyButton.interactable = !state.IsReady;
			}
		}

		[ClientRpc]
		private void ServerTimeUpdatedClientRpc(float currentTime){
			timeBarMask.localScale = new Vector3(currentTime / readyTime, 1, 1);
		}
	}
}