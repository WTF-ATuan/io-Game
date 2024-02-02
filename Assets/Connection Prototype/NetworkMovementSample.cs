using Unity.Netcode;
using UnityEngine;

namespace Assets.Scenes{
	public class NetworkMovementSample : NetworkBehaviour{
		private void Update(){
			if(!IsOwner){
				return;
			}
			HandleMovementServerAuth();
		}

		private void HandleMovementServerAuth(){
			var inputVector = new Vector2();
			HandleMovementServerRpc(inputVector);
		}

		[ServerRpc(RequireOwnership = false)] // RequireOwnership = 是否一定要是Server 才能發?
		private void HandleMovementServerRpc(Vector2 inputValue){
			//告訴Net work Transform (Client RPC) 要移動
		}
	}
}