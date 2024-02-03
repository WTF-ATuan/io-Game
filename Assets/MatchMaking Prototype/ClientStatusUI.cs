using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scenes{
	public class ClientStatusUI : MonoBehaviour{
		[SerializeField] private Button matchButton;
		[SerializeField] private TMP_Text connectingText;
		[SerializeField] private TMP_Text playerList;

		private int _timer;
		public void OnStartMatch(){
			matchButton.gameObject.SetActive(false);
			connectingText.gameObject.SetActive(true);
		}

		public void OnWaitingConnection(){
			_timer += 1;
			var timeSpan = TimeSpan.FromSeconds(_timer);
			connectingText.SetText($"Connecting....{timeSpan:mm\\:ss}");
		}

		public void OnConnect(){
			connectingText.gameObject.SetActive(false);
			playerList.transform.parent.gameObject.SetActive(true);
			PlayerIDListRequestServerRPC();
		}

		//Server 還沒有這段的版本Build 所以還不能觸發
		[ServerRpc(RequireOwnership = false)]
		private void PlayerIDListRequestServerRPC(){
			var connectedClients = NetworkManager.Singleton.ConnectedClients;
			foreach(var client in connectedClients){
				var clientID = client.Key;
				playerList.text += $"Player ID : {clientID} \n";
			}
		}



	}
}