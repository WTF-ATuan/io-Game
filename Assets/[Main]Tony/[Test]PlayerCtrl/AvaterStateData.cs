using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvaterStateData{
	public float TimeStamp;

	public Vector2 Pos;
	public Vector2 TargetVec;
	public Vector2 NowVec;
	public Vector2 AimPos;
	public float Towards;
	public float RotVec;

	public float Power; //0~1
	public float ShootCd;

	private readonly IInput _input;
	private readonly PlayerLoadout _loadout;
	private readonly Transform _playerTransform;
	private readonly AvaterAttribute _attribute;

	public bool IsAim => AimPos != Vector2.zero;
	
	public AvaterStateData(IInput input, PlayerLoadout loadout, Transform playerTransform
		, AvaterAttribute attribute){
		_input = input;
		_loadout = loadout;
		_playerTransform = playerTransform;
		_attribute = attribute;

		TimeStamp = Time.time; //todo change to serverSyncTime
		Pos = _playerTransform.position;
		Power = 1;
	}

	public void ServerDataRefresh(AvaterStateData data){
		Pos = data.Pos;
		TargetVec = data.TargetVec;
		NowVec = data.NowVec;
		Towards = data.Towards;
		RotVec = data.RotVec;
	}

	public void ClientDataRefresh(){
		float nowTime = Time.time;
		Pos = (Vector2)_playerTransform.position;
		TargetVec = _input.MoveJoy();
		var newAimPos = _input.AimJoy();

		if(AimPos != Vector2.zero && newAimPos == Vector2.zero){
			if(ShootCd > nowTime) return;
			int maxBullet = _loadout.NowAttribute.MaxBullet;
			if(maxBullet > 0){
				float powerNeed = 1f / maxBullet;
				if(Power >= powerNeed){
					//todo Shoot
					Power = Mathf.Clamp01(Power - powerNeed);
					ShootCd = nowTime + _loadout.NowAttribute.ShootCD;
					Towards = AimPos.Angle();
					RotVec = 0;
				}
			}
		}

		AimPos = newAimPos;
	}

	public void LocalUpdate(){
		float missTime = Time.time - TimeStamp;
		TimeStamp = Time.time; //todo change to serverSyncTime

		Vector2 vec = TargetVec - NowVec;
		Vector2 direction = vec.normalized;
		Vector2 newVec = TargetVec;
		float distance = vec.magnitude;
		float moveFriction = AvaterAttribute.MoveFriction;
		if(distance > moveFriction){
			newVec = NowVec + direction * Mathf.Min(moveFriction, distance);
		}

		NowVec = newVec;
		Pos = Pos + NowVec * _attribute.MoveSpeed * missTime;

		//Towards
		float targetTowards =  !IsAim
				? TargetVec != Vector2.zero ? TargetVec.Angle() : Towards
				: AimPos.Angle();
		Towards = Mathf.SmoothDampAngle(Towards, targetTowards, ref RotVec, AvaterAttribute.RotSpeed);

		//Power
		Power = Mathf.Clamp01(Power + missTime / _loadout.NowAttribute.PowerChargeToFullSec);
	}
}