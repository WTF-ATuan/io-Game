using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

namespace Assets.Scenes{
	public class ConnectionManager : MonoBehaviour{
		private const string InternalServerIp = "0.0.0.0";
		private ushort _severDefaultPort = 7777;
		private const ushort SeverMaxPlayer = 10;
		private const int MultiplayServerTimeout = 20000;

		private IMultiplayService _multiplayService;
		private string _allocationID;
		private string _externalServerIp = "0.0.0.0";
		private string ExternalConnectString => $"{_externalServerIp}:{_severDefaultPort}";


		private async void Start(){
			var isStartServer = false;
			var args = System.Environment.GetCommandLineArgs();
			for(var index = 0; index < args.Length; index++){
				var arg = args[index];
				if(arg == "-dedicatedServer"){
					isStartServer = true;
				}

				if(arg == "-port" && index + 1 < args.Length){
					_severDefaultPort = ushort.Parse(args[index + 1]);
				}

				if(arg == "-ip" && index + 1 < args.Length){
					_externalServerIp = args[index + 1];
				}
			}

			if(isStartServer){
				StartServer();
				await StartServerServices();
			}
		}

		private void StartServer(){
			NetworkManager.Singleton.GetComponent<UnityTransport>()
					.SetConnectionData(InternalServerIp, _severDefaultPort);
			NetworkManager.Singleton.StartServer();
		}

		private async Task StartServerServices(){
			await UnityServices.InitializeAsync();
			try{
				_multiplayService = MultiplayService.Instance;
				await _multiplayService.StartServerQueryHandlerAsync(SeverMaxPlayer, "n/a"
					, "n/a", "0", "n/a");
			}
			catch(Exception exception){
				Debug.LogWarning($"SQP Service Exception:\n {exception}");
			}

			try{
				var matchmakerPayload = await GetMatchmakerPayload(MultiplayServerTimeout);
				Debug.Log($"Got the payload Start doing Backfill {matchmakerPayload}");
				await StartBackFill(matchmakerPayload);
			}
			catch(Exception exception){
				Debug.LogWarning($"Allocation & Backfill Services Exception:\n {exception}");
			}
		}

		private async Task<MatchmakingResults> GetMatchmakerPayload(int timeout){
			var matchmakingResults = ProcessingMatchMakerAllocation();
			return await Task.WhenAny(matchmakingResults, Task.Delay(timeout)) == matchmakingResults
					? matchmakingResults.Result
					: throw new Exception("Matchmaking payload is null please check");
		}

		private async Task<MatchmakingResults> ProcessingMatchMakerAllocation(){
			if(_multiplayService == null) throw new Exception("Play Service is not Connect");
			_allocationID = await GetAllocationIDAsync();
			return await GetMatchmakerAllocationPayloadAsync();
		}

		private async Task<MatchmakingResults> GetMatchmakerAllocationPayloadAsync(){
			try{
				var payloadAllocation = await _multiplayService.GetPayloadAllocationFromJsonAs<MatchmakingResults>();
				Debug.Log(
					$"Match making allocation {JsonConvert.SerializeObject(payloadAllocation, Formatting.Indented)}");
				return payloadAllocation;
			}
			catch(Exception exception){
				Debug.LogWarning($"Match maker payload Exception:\n {exception}");
			}

			throw new Exception($"Can,t get allocation payload from {_multiplayService}");
		}

		private async Task<string> GetAllocationIDAsync(){
			_allocationID = string.Empty;
			var config = _multiplayService.ServerConfig;
			Debug.Log($"Awaiting server config log:\n" +
					  $"-Server ID:{config.ServerId}\n" +
					  $"-Allocation ID:{config.AllocationId}\n" +
					  $"-Port:{config.Port}\n" +
					  $"-QPort:{config.QueryPort}\n" +
					  $"-Log:{config.ServerLogDirectory}\n"
			);
			while(string.IsNullOrEmpty(_allocationID)){
				if(!string.IsNullOrEmpty(config.AllocationId)){
					_allocationID = config.AllocationId;
					break;
				}

				await Task.Delay(100);
			}

			return _allocationID;
		}

		private async Task StartBackFill(MatchmakingResults payload){
			var ticketProperties = new BackfillTicketProperties(payload.MatchProperties);
			var backfillTicket =
					new BackfillTicket(id: payload.MatchProperties.BackfillTicketId, properties: ticketProperties);
			await BackFilling(payload, backfillTicket);
		}

		private async Task BackFilling(MatchmakingResults payload, BackfillTicket ticket){
			var ticketOptions = new CreateBackfillTicketOptions{
				Connection = ExternalConnectString,
				QueueName = payload.QueueName,
				Properties = new BackfillTicketProperties(payload.MatchProperties)
			};
			ticket.Id = await MatchmakerService.Instance.CreateBackfillTicketAsync(ticketOptions);
			
			await BackFillLoop(ticket);
		}

		private async Task BackFillLoop(BackfillTicket ticket){
			while(!IsServerFull()){
				ticket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(ticket.Id);
				if(IsServerFull()){
					await MatchmakerService.Instance.DeleteBackfillTicketAsync(ticket.Id);
					ticket.Id = string.Empty;
					return;
				}

				await Task.Delay(1000);
			}
		}

		private bool IsServerFull(){
			return NetworkManager.Singleton.ConnectedClients.Count >= SeverMaxPlayer;
		}
	}
}