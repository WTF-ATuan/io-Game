using Unity.Netcode.Components;

namespace Networking_Feature.Player{
	public class ClientNetworkTransform : NetworkTransform{
		protected override bool OnIsServerAuthoritative(){
			return false;
		}
	}
}