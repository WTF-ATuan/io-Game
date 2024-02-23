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
			playerList.text = $"Player ID : {NetworkManager.Singleton.LocalClientId} \n";
		}
	}
}