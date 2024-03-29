using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameStatusHandler{
	private readonly Dictionary<ulong, PlayerInGameData> _inGameDataRepository = new ();
	public GameStatusHandler(){
		MatchplayNetworkServer.Instance.OnPlayerJoined += OnPlayerJoined;
		MatchplayNetworkServer.Instance.OnPlayerLeft += OnPlayerLeft;
	}

	private void OnPlayerJoined(UserData data){
		var clientId = data.clientId;
		var clientOwnedObjects = NetworkManager.Singleton.SpawnManager.GetClientOwnedObjects(clientId);
		var playerObject = clientOwnedObjects.Find(x => x.TryGetComponent<IGetLoadOut>(out var loadOut));
		var playerInGameData = new PlayerInGameData(clientId , 100 , playerObject.GetComponent<IGetLoadOut>());
		_inGameDataRepository.Add(clientId , playerInGameData);
	}

	private void OnPlayerLeft(UserData data){
		_inGameDataRepository.Remove(data.clientId);
	}
}
public class PlayerInGameData{
	public ulong ClientID;
	public int GameHealth;
	public IGetLoadOut AvatarLoadOut;

	public PlayerInGameData(ulong clientID, int gameHealth, IGetLoadOut avatarLoadOut){
		ClientID = clientID;
		GameHealth = gameHealth;
		AvatarLoadOut = avatarLoadOut;
	}
}