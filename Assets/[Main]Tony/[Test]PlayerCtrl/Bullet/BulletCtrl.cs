using System;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class BulletCtrl : NetworkBehaviour{
	private readonly NetworkVariable<Vector3> _movement = new();
	private NetworkValue.Vec3Smoother _vec3Smoother;

	public override void OnNetworkSpawn(){
		_vec3Smoother = new NetworkValue.Vec3Smoother(() => _movement.Value, () => transform.position);
		if(IsClient){
			_movement.OnValueChanged += (_, _) => {
				_vec3Smoother.Update();
			};
		}

		if(IsServer){
			var position = transform.position;
			_movement.Value = new Vector3(position.x , position.y , position.z);
		}

	}

	public void Setup(Vector2 genPos, float angle, float moveSec, float maxDis, Action onFinish){
		transform.position = genPos;
		transform.eulerAngles = Vector3.forward * angle;
		transform.DOMove(genPos + angle.ToVec2() * maxDis, moveSec)
				.SetUpdate(UpdateType.Fixed)
				.OnComplete(onFinish.Invoke);
	}

	private void Update(){
		if(IsServer){
			var position = transform.position;
			_movement.Value = new Vector3(position.x, position.y, position.z);
		}
		else{
			transform.position = _vec3Smoother.Get();
		}
	}
}