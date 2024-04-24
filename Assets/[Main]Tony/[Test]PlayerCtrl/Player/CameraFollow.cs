using System;
using Unity.Netcode;
using UnityEngine;

namespace _Main_Tony._Test_PlayerCtrl.Player{
	public class CameraFollow : MonoBehaviour{
		public Transform target; // 要跟隨的目標物件
		public float smoothSpeed = 0.125f; // 平滑移動的速度
		public Vector3 offset; // 相機與目標物件的偏移量

		private void FixedUpdate()
		{
			if(target == null){
				if(NetworkManager.Singleton.LocalClient.PlayerObject == null){
					return;
				}
				target = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
				offset = transform.position;
				return;
			}

			var desiredPosition = target.position + offset;
			var smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
			transform.position = smoothedPosition;
			
		}
	}
}