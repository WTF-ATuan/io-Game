using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyEventHandler : NetworkBehaviour{
	[SerializeField] private VerticalLayoutGroup playerTabList;
	[SerializeField] private GameObject playerTabPrefab;

	private NetworkList<PlayerLobbyState> _playerStatesList;

	private void Awake(){
		_playerStatesList = new NetworkList<PlayerLobbyState>();
	}

	public override void OnNetworkSpawn(){
		if(IsServer){
			HandleServer();
		}
		else{
			_playerStatesList.OnListChanged += x => UpdatePlayerTab();
		}
	}

	public void Ready(){
		ReadyRequestServerRPC();
	}

	[ServerRpc(RequireOwnership = false)]
	private void ReadyRequestServerRPC(ServerRpcParams serverRpcParams = default){
		for(var index = 0; index < _playerStatesList.Count; index++){
			var state = _playerStatesList[index];
			if(serverRpcParams.Receive.SenderClientId != state.ClientID) continue;
			_playerStatesList[index] = new PlayerLobbyState(state.ClientID, state.ViewID, true);
			UpdatePlayerTab();
		}

		foreach(var player in _playerStatesList){
			if(!player.IsReady){
				return;
			}
		}
		//Todo Set player Character info

		MatchplayNetworkServer.Instance.StartGame();
	}

	private void HandleServer(){
		NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
		NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
	}

	private void HandleClientConnected(ulong clientID){
		var playerTab = Instantiate(playerTabPrefab, Vector3.zero, Quaternion.identity);
		playerTab.GetComponent<NetworkObject>().Spawn();
		playerTab.transform.SetParent(playerTabList.transform);
		_playerStatesList.Add(new PlayerLobbyState(clientID, playerTab.GetComponent<NetworkObject>().NetworkObjectId));
		UpdatePlayerTab();
	}

	private void HandleClientDisconnected(ulong clientID){
		PlayerLobbyState lobbyState = default;
		foreach(var state in _playerStatesList){
			if(state.ClientID == clientID){
				lobbyState = state;
			}
		}

		var foundChild = NetworkManager.Singleton.SpawnManager.SpawnedObjects[lobbyState.ViewID];
		foundChild.Despawn();
		_playerStatesList.Remove(lobbyState);
	}

	private void UpdatePlayerTab(){
		foreach(var state in _playerStatesList){
			Debug.Log($"{state.ClientID} {state.ViewID}");
			var playerTab = NetworkManager.Singleton.SpawnManager.SpawnedObjects[state.ViewID];
			var text = playerTab.GetComponentInChildren<Text>();
			var image = playerTab.GetComponentsInChildren<Image>()[1];
			text.text =
					$"Player : {state.ClientID} ";
			image.color = state.IsReady ? Color.red : Color.green;
		}
	}
}

public struct PlayerLobbyState : INetworkSerializable, IEquatable<PlayerLobbyState>{
	public ulong ClientID;
	public bool IsReady;
	public ulong ViewID;

	public PlayerLobbyState(ulong clientID, ulong viewID, bool isReady = false){
		ClientID = clientID;
		ViewID = viewID;
		IsReady = isReady;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter{
		serializer.SerializeValue(ref ClientID);
		serializer.SerializeValue(ref ViewID);
		serializer.SerializeValue(ref IsReady);
	}

	public bool Equals(PlayerLobbyState other){
		return other.ClientID == ClientID;
	}
}