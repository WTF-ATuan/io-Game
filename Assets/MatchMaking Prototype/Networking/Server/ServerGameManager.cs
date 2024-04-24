using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class ServerGameManager : IDisposable{
	private MultiplayAllocationService multiplayAllocationService;
	private SynchedServerData synchedServerData;
	string ConnectionString => $"{serverIP}:{serverPort}";
	private string serverIP = "0.0.0.0";
	private int serverPort = 7777;
	private int queryPort = 7787;
	private string serverName = "Matchplay Server";
	private bool startedServices;

	private int playerCount;

	private MatchmakingResults matchmakerPayload;

	public MatchplayNetworkServer NetworkServer{ get; private set; }

	private const int MultiplayServiceTimeout = 20000;

	public ServerGameManager(string serverIP, int serverPort, int serverQPort, NetworkManager manager){
		this.serverIP = serverIP;
		this.serverPort = serverPort;
		this.queryPort = serverQPort;
		NetworkServer = new MatchplayNetworkServer(manager);
		multiplayAllocationService = new MultiplayAllocationService();
		serverName = $"Server: {Guid.NewGuid()}";
	}

	public async Task StartGameServerAsync(GameInfo startingGameInfo){
		Debug.Log($"Starting server with:{startingGameInfo}.");

		await multiplayAllocationService.BeginServerCheck();

		try{
			matchmakerPayload = await GetMatchmakerPayload(MultiplayServiceTimeout);

			if(matchmakerPayload != null){
				Debug.Log($"Got payload: {matchmakerPayload}");
				startingGameInfo = PickGameInfo(matchmakerPayload);

				SetAllocationData(startingGameInfo);
				NetworkServer.OnPlayerJoined += UserJoinedServer;
				NetworkServer.OnPlayerLeft += UserLeft;
				startedServices = true;
				
				#pragma warning disable CS4014
				StartBackfill(matchmakerPayload);
				#pragma warning restore CS4014
			}
			else{
				Debug.LogWarning("Getting the Matchmaker Payload timed out, starting with defaults.");
			}
		}
		catch(Exception ex){
			Debug.LogWarning($"Something went wrong trying to set up the Services:\n{ex} ");
		}

		if(!NetworkServer.OpenConnection(serverIP, serverPort, startingGameInfo)){
			Debug.LogError("NetworkServer did not start as expected.");
			return;
		}

		synchedServerData = await NetworkServer.ConfigureServer(startingGameInfo);
		if(synchedServerData == null){
			Debug.LogError("Could not find the synchedServerData.");
			return;
		}

		synchedServerData.serverId.Value = serverName;

		synchedServerData.map.OnValueChanged += OnServerChangedMap;
		synchedServerData.gameMode.OnValueChanged += OnServerChangedMode;
	}

	private async Task<MatchmakingResults> GetMatchmakerPayload(int timeout){
		if(multiplayAllocationService == null){
			return null;
		}

		var matchmakerPayloadTask = multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

		if(await Task.WhenAny(matchmakerPayloadTask, Task.Delay(timeout)) == matchmakerPayloadTask){
			return matchmakerPayloadTask.Result;
		}

		return null;
	}

	private void SetAllocationData(GameInfo startingGameInfo){
		multiplayAllocationService.SetServerName(serverName);
		multiplayAllocationService.SetMaxPlayers(20);
		multiplayAllocationService.SetBuildID("0");
		multiplayAllocationService.SetMap(startingGameInfo.map.ToString());
		multiplayAllocationService.SetMode(startingGameInfo.gameMode.ToString());
	}

	public static GameInfo PickGameInfo(MatchmakingResults mmAllocation){
		GameQueue queue = GameInfo.ToGameQueue(mmAllocation.QueueName);
		return new GameInfo{ map = Map.Default, gameMode = GameMode.Default, gameQueue = queue };
	}

	private void OnServerChangedMap(Map oldMap, Map newMap){
		multiplayAllocationService.SetMap(newMap.ToString());
	}

	private void OnServerChangedMode(GameMode oldMode, GameMode newMode){
		multiplayAllocationService.SetMode(newMode.ToString());
	}

	private void UserJoinedServer(UserData joinedUser){
		Debug.Log($"{joinedUser} joined the game");
		multiplayAllocationService.AddPlayer();
		playerCount++;
	}

	private void UserLeft(UserData leftUser){
		multiplayAllocationService.RemovePlayer();
		playerCount--;

		if(playerCount > 0){
			if(NeedBackfill()){
				#pragma warning disable CS4014
				StartBackfill(matchmakerPayload);
				#pragma warning restore CS4014
			}
			return;
		}

		CloseServer();
	}

	private void CloseServer(){
		Debug.Log("Closing Server");
		NetworkLog.LogInfo("Closing Server");
		Dispose();
		NetworkManager.Singleton.Shutdown();
		Application.Quit();
	}

	private BackfillTicket _backfillTicket;
	private CreateBackfillTicketOptions _createTicketOptions;

	private async Task StartBackfill(MatchmakingResults payload){
		_backfillTicket = new BackfillTicket{
			Id = payload.MatchProperties.BackfillTicketId,
			Properties = new BackfillTicketProperties(payload.MatchProperties)
		};
		if(string.IsNullOrEmpty(_backfillTicket.Id)){
			_createTicketOptions = new CreateBackfillTicketOptions{
				Connection = ConnectionString,
				QueueName = payload.QueueName,
				Properties = new BackfillTicketProperties(payload.MatchProperties)
			};
			_backfillTicket.Id = await MatchmakerService.Instance.CreateBackfillTicketAsync(_createTicketOptions);
		}

		#pragma warning disable CS4014
		BackfillLoop();
		#pragma warning restore CS4014
	}

	private async Task BackfillLoop(){
		const int backfillCheckInterval = 1;
		while(NeedBackfill()){
			_backfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(_backfillTicket.Id);
			if(!NeedBackfill()){
				await MatchmakerService.Instance.DeleteBackfillTicketAsync(_backfillTicket.Id);
				_backfillTicket.Id = string.Empty;
				break;
			}

			await Task.Delay(TimeSpan.FromSeconds(backfillCheckInterval));
		}
	}


	private bool NeedBackfill(){
		return playerCount <= 20;
	}

	public void Dispose(){
		if(startedServices){
			if(NetworkServer.OnPlayerJoined != null) NetworkServer.OnPlayerJoined -= UserJoinedServer;
			if(NetworkServer.OnPlayerLeft != null) NetworkServer.OnPlayerLeft -= UserLeft;
		}

		if(synchedServerData != null){
			if(synchedServerData.map.OnValueChanged != null)
				synchedServerData.map.OnValueChanged -= OnServerChangedMap;
			if(synchedServerData.gameMode.OnValueChanged != null)
				synchedServerData.gameMode.OnValueChanged -= OnServerChangedMode;
		}

		multiplayAllocationService?.Dispose();
		NetworkServer?.Dispose();
	}
}