using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

namespace Assets.Scenes{
	public class MatchMakingClient : MonoBehaviour{
		[SerializeField] private ClientStatusUI statusUI;
		
		private void OnEnable(){
			ConnectionManager.ClientSignin += SignIn;
		}

		private void OnDisable(){
			ConnectionManager.ClientSignin -= SignIn;
		}

		private async void SignIn(){
			var initializationOptions = new InitializationOptions();
			await UnityServices.InitializeAsync(initializationOptions);
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
			Debug.Log($"Player Sign in with {PlayerID()}");
		}

		public void StartMatch(){
			CreatATicket();
			statusUI.OnStartMatch();
		}

		private async void CreatATicket(){
			var options = new CreateTicketOptions("test-battleMode");
			var players = new List<Player>{
				new(PlayerID(), new MatchMakingPlayerData{
					skill = 100
				})
			};
			var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
			Debug.Log($"ticketResponse.id = {ticketResponse.Id}");
			CheckTicketStatus(ticketResponse.Id);
		}

		private async void CheckTicketStatus(string ticketID){
			var gotAssignment = false;
			MultiplayAssignment multiplayAssignment = null;
			do{
				await Task.Delay(TimeSpan.FromSeconds(1));
				var ticket = await MatchmakerService.Instance.GetTicketAsync(ticketID);
				if(ticket == null) continue;
				if(ticket.Type == typeof(MultiplayAssignment)){
					multiplayAssignment = ticket.Value as MultiplayAssignment;
				}

				switch(multiplayAssignment.Status){
					case MultiplayAssignment.StatusOptions.Timeout:
						throw new Exception($"Ticket Fail: Time out");
					case MultiplayAssignment.StatusOptions.Failed:
						throw new Exception($"Ticket Fail: {multiplayAssignment.Message}");
					case MultiplayAssignment.StatusOptions.InProgress:
						statusUI.OnWaitingConnection();
						break;
					case MultiplayAssignment.StatusOptions.Found:
						gotAssignment = true;
						TicketAssignment(multiplayAssignment);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			} while(!gotAssignment);
		}

		private void TicketAssignment(MultiplayAssignment assignment){
			NetworkManager.Singleton.GetComponent<UnityTransport>()
					.SetConnectionData(assignment.Ip, (ushort)assignment.Port);
			NetworkManager.Singleton.StartClient();
			Debug.Log($"Ip:{assignment.Ip} , Port:{assignment.Port}");
			statusUI.OnConnect();
		}

		private string PlayerID(){
			return AuthenticationService.Instance.PlayerId;
		}
	}

	[Serializable]
	public class MatchMakingPlayerData{
		public int skill;
	}
}