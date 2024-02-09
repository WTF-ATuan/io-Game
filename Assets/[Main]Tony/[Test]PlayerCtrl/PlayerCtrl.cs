using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zenject;


public class PlayerCtrl : NetworkBehaviour{
	public Transform body;

	private IInput InputCtrl{ get; set; }
	private AvaterAttribute BaseAttribute{ get; set; }
	private AvaterStateData StateData{ get; set; }
	private PlayerLoadout Loadout{ get; set; }
	private PoolObj<HealthBarCtrl> HealthBar{ get; set; }

	private List<IDisposable> _recycleThings;
	private RangePreviewCtrl RangePreview;

	private NetworkVariable<Vector3> _syncPosition = new();
	private NetworkVariable<float> _syncTowardEuler = new();

	[Inject]
	private void Initialization(
		IAvaterAttributeCtrl avaterAttributeCtrl,
		IInput inputCtrl,
		IBattleCtrl battleCtrl,
		ObjPoolCtrl<HealthBarCtrl> healthBarPool,
		IWeaponFactory weaponFactory
	){
		battleCtrl.SetLocalPlayer(this);
		_recycleThings = new List<IDisposable>();
		InputCtrl = inputCtrl;
		BaseAttribute = avaterAttributeCtrl.GetData();
		Loadout = new PlayerLoadout(BaseAttribute);

		StateData = new AvaterStateData(InputCtrl, Loadout, transform, BaseAttribute);
		HealthBar = healthBarPool.Get();
		HealthBar.Ctrl.Setup(Loadout.NowAttribute, StateData);
		HealthBar.Obj.transform.parent = transform;
		_recycleThings.Add(HealthBar);
		RangePreview = GetComponentInChildren<RangePreviewCtrl>();

		var weapon =
				weaponFactory.Create<Shotgun>(3, 6, 1000, 0.5f, new RangePreviewData{ Radius = 1, SectorAngle = 0.1f });
		Loadout.SetWeapon(weapon, out var unload);
	}

	public override void OnDestroy(){
		base.OnDestroy();
		foreach(var thing in _recycleThings){
			thing.Dispose();
		}
	}

	private void FixedUpdate(){
		if(IsOwner && IsClient){
			CalculateActionData();
		}

		UpdateClientData();
	}

	private void CalculateActionData(){
		MovementRequestServerRpc(CalculatePlayerMove(), CalculatePlayerRotate());
	}

	[ServerRpc(RequireOwnership = false)]
	private void MovementRequestServerRpc(Vector3 position, float towardEuler){
		_syncPosition.Value = position;
		_syncTowardEuler.Value = towardEuler;
	}

	private void UpdateClientData(){
		transform.position = _syncPosition.Value;
		body.eulerAngles = new Vector3(0, 0, _syncTowardEuler.Value);
	}

	private Vector2 _nowVec;

	private double _timeStamp;

	private Vector3 CalculatePlayerMove(){
		var missTime = NetworkManager.ServerTime.Time - _timeStamp;
		_timeStamp = NetworkManager.ServerTime.Time;
		var targetVec = InputCtrl.MoveJoy();
		var vec = targetVec - _nowVec;
		var direction = vec.normalized;
		var newVec = targetVec;
		var distance = vec.magnitude;
		const float moveFriction = AvaterAttribute.MoveFriction;
		if(distance > moveFriction){
			newVec = _nowVec + direction * Mathf.Min(moveFriction, distance);
		}

		_nowVec = newVec;
		Vector2 pos = transform.position;
		pos += _nowVec * BaseAttribute.MoveSpeed * (float)missTime;
		return pos;
	}

	private float _towards;

	private float CalculatePlayerRotate(){
		var targetVec = InputCtrl.MoveJoy();
		var aimPos = InputCtrl.AimJoy();
		float refVec = 0;
		var targetTowards = aimPos != Vector2.zero
				? targetVec != Vector2.zero ? targetVec.Angle() : _towards
				: aimPos.Angle();
		_towards = Mathf.SmoothDampAngle(_towards, targetTowards, ref refVec, AvaterAttribute.RotSpeed);
		return _towards;
	}
}