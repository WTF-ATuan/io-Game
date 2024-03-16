using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scenes{
	public class MatchMakingMenu : MonoBehaviour{
		private ClientGameManager _clientGameManager;

		[SerializeField] private Button matchButton;
		[SerializeField] private TMP_Text connectingText;
		[SerializeField] private TMP_Text playerIdText;


		private void Start(){
			if(_clientGameManager == null) return;
			_clientGameManager = ClientSingleton.Instance.Manager;
		}

		public async void StartMatch(){
			matchButton.gameObject.SetActive(false);
			connectingText.gameObject.SetActive(true);
			connectingText.text = "Connecting...";
			await _clientGameManager.MatchmakeAsync(OnMatchMade);
		}

		private void OnMatchMade(MatchmakerPollingResult result){
			connectingText.gameObject.SetActive(false);
			playerIdText.transform.parent.gameObject.SetActive(true);
			playerIdText.text = $"Player ID : {_clientGameManager.User.AuthId}";
		}
	}
}