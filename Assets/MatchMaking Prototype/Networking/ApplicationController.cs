using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;

public class ApplicationController : MonoBehaviour{
	[Header("Setting")] [SerializeField] private bool editorTestingMode = true;
	[Header("Setting")] [SerializeField] private bool isHostMode = true;

	[Header("References")] [SerializeField]
	private ServerSingleton serverPrefab;

	[SerializeField] private ClientSingleton clientPrefab;
	[SerializeField] private HostSingleton hostSingleton;

	private ApplicationData appData;
	public static bool IsServer;

	private async void Start(){
		Application.targetFrameRate = 60;
		DontDestroyOnLoad(gameObject);

		await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
	}

	private async Task LaunchInMode(bool isServer){
		appData = new ApplicationData();
		IsServer = isServer;

		if(isServer){
			ServerSingleton serverSingleton = Instantiate(serverPrefab);
			await serverSingleton.CreateServer();

			var defaultGameInfo = new GameInfo{
				gameMode = GameMode.Default,
				map = Map.Default,
				gameQueue = GameQueue.Casual
			};

			await serverSingleton.Manager.StartGameServerAsync(defaultGameInfo);
		}
		else{
			ClientSingleton clientSingleton = Instantiate(clientPrefab);
			Instantiate(hostSingleton);

			await clientSingleton.CreateClient();
			if(editorTestingMode){
				clientSingleton.Manager.ToMainMenu();
			}
			else{
				if(isHostMode){
					await hostSingleton.StartHostAsync();
				}
				else{
					await FindHost();
				}
			}
		}
	}

	private async Task FindHost(){
		AuthenticationWrapper.SignOut();
		AuthenticationService.Instance.SwitchProfile("PlayerEditor");
		await AuthenticationService.Instance.SignInAnonymouslyAsync();
		ClientSingleton.Instance.Manager.User.AuthId = "PlayerEditor";
		
		var findHost = false;
		const int requestLimits = 30;
		var times = 0;
		
		while(!findHost){
			var lobbies = await Lobbies.Instance.QueryLobbiesAsync();
			if(lobbies.Results.Count < 1){
				await Task.Delay(1000);
				times++;
				if(times > requestLimits) break;
				continue;
			}

			foreach(var lobby in lobbies.Results){
				Debug.Log($"{lobbies.Results.Count}");
				Debug.Log($"{lobby.Name} {lobby.Id} {lobby.LobbyCode}");
				var joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
				var joinCode = joiningLobby.Data["JoinCode"].Value;
				await ClientSingleton.Instance.Manager.BeginConnection(joinCode);
				findHost = true;
				break;
			}
		}
	}
}