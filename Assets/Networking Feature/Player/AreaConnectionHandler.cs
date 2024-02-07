using Unity.Netcode;
using UnityEngine;

namespace Networking_Feature.Player{
	public class AreaConnectionHandler : MonoBehaviour{
		
		
		
		/// <summary>
		/// host => 1 , client => 2 , server => 3
		/// </summary>
		/// <param name="auth"></param>
		public void StartWith(int auth){
			switch(auth){
				case 1:
					NetworkManager.Singleton.StartHost();
					break;
				case 2:
					NetworkManager.Singleton.StartClient();
					break;
			}
		}
	}
}