using UnityEngine;
using Zenject;

namespace Networking_Feature.Player{
	public class NetworkingMovement : MonoBehaviour{
		[Inject] private IInput _localPlayerInput;
		[Inject] private PlayerActionData _playerActionData;
		
		
	}
}