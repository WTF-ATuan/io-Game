using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Assets.Scenes{
	public class ConnectionManager : MonoBehaviour{
		private const string SeverInternalIp = "0.0.0.0";
		private ushort _severDefaultPort = 7777;


		private void Start(){
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
			}

			if(isStartServer) StartServer();
		}

		private void StartServer(){
			NetworkManager.Singleton.GetComponent<UnityTransport>()
					.SetConnectionData(SeverInternalIp, _severDefaultPort);
			NetworkManager.Singleton.StartServer();
		}


		private void OnGUI(){
			ShowPlayers();
		}

		private void ShowPlayers(){
			var guiStyle = new GUIStyle{
				fontSize = 30,
			};
			GUILayout.Label($"Sever time: {NetworkManager.Singleton.ServerTime.Time}", guiStyle);
			var connectedClients = NetworkManager.Singleton.ConnectedClients;
			foreach(var client in connectedClients){
				GUILayout.Label($"PlayerID: {client.Key}", guiStyle);
			}
		}
	}
}