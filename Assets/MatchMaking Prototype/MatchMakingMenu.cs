using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scenes{
	public class MatchMakingMenu : MonoBehaviour{
		[SerializeField] private Button matchButton;
		[SerializeField] private TMP_Text connectingText;
		[SerializeField] private TMP_Text playerIdText;
		

		public async void StartMatch(){
			matchButton.gameObject.SetActive(false);
			connectingText.gameObject.SetActive(true);
			connectingText.text = "Connecting...";
			await ClientSingleton.Instance.Manager.MatchmakeAsync(OnMatchMade);
		}

		private void OnMatchMade(MatchmakerPollingResult result){
			connectingText.gameObject.SetActive(false);
			playerIdText.transform.parent.gameObject.SetActive(true);
			playerIdText.text = $"Player ID : {ClientSingleton.Instance.Manager.User.AuthId}";
		}
	}
}