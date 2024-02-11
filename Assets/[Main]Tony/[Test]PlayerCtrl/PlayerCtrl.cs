using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zenject;


public class PlayerCtrl : NetworkBehaviour,IAvaterSync{
	public Transform body;

	private IInput InputCtrl{ get; set; }
	private AvaterAttribute BaseAttribute{ get; set; }
	private AvaterStateData StateData{ get; set; }
	private PlayerLoadout Loadout{ get; set; }
	private PoolObj<HealthBarCtrl> HealthBar{ get; set; }

	private List<IDisposable> _recycleThings;
	private RangePreviewCtrl RangePreview;
	private AvaterStateData2 StateData2{ get; set; }

	[Inject]
	private void Initialization(
		IAvaterAttributeCtrl avaterAttributeCtrl,
		IInput inputCtrl,
		IBattleCtrl battleCtrl,
		ObjPoolCtrl<HealthBarCtrl> healthBarPool,
		IWeaponFactory weaponFactory
	){
		battleCtrl.SetLocalPlayer(this); //Todo add condition => if(currentPlayer == ownerPlayer)
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

		var weapon = weaponFactory.Create<Shotgun>(3, 6, 1000, 0.5f,new RangePreviewData{Type = RangePreviewType.Sector,Dis = 6,Width = 36});
		Loadout.SetWeapon(weapon, out var unload);
		
		///
		StateData2 = new AvaterStateData2(this);
	}

	public override void OnDestroy(){
		base.OnDestroy();
		foreach(var thing in _recycleThings){
			thing.Dispose();
		}
	}
	
	private NetworkVariable<AvaterSyncData3> AvaterSyncData = new();
	public NetworkVariable<AvaterSyncData3> GetSyncData() {
		return AvaterSyncData;
	}
	
	[ServerRpc(RequireOwnership = false)]
	public void AvaterDataSyncServerRpc(AvaterSyncData3 data) {
		AvaterSyncData.Value = data;
	}
	
	public PlayerLoadout GetLoadOut() {
		return Loadout;
	}
	
	public Transform GetTransform() {
		return transform;
	}

	public IInput GetInput() {
		return InputCtrl;
	}

	public bool IsOwner() {
		return base.IsOwner && base.IsClient;
	}

	private void Update() {
		StateData2.DataSync();
	}

	private void FixedUpdate() {
		StateData2.ClientUpdate();
	}

	/*
	[ServerRpc(RequireOwnership = false)]
	private void MovementRequestServerRpc(Vector3 position, float towardEuler){
		_syncPosition.Value = position;
		_syncTowardEuler.Value = towardEuler;
	}
	
	private NetworkVariable<Vector3> _syncPosition = new();
	private NetworkVariable<float> _syncTowardEuler = new();
	
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
		transform.position = Vector3.Lerp(transform.position, _syncPosition.Value,
			NetworkManager.ServerTime.FixedDeltaTime * 2);
		body.eulerAngles = new Vector3(0, 0, _syncTowardEuler.Value);
	}

	private Vector2 _nowVec;

	private double _timeStamp;

	private Vector3 CalculatePlayerMove(){
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
		pos += _nowVec * BaseAttribute.MoveSpeed;
		return pos;
	}

	private float _towards;

	private float CalculatePlayerRotate(){
		var targetVec = InputCtrl.MoveJoy();
		var aimPos = InputCtrl.AimJoy();
		float refVec = 0;
		var targetTowards = !(aimPos != Vector2.zero)
				? targetVec != Vector2.zero ? targetVec.Angle() : _towards
				: aimPos.Angle();
		_towards = Mathf.SmoothDampAngle(_towards, targetTowards, ref refVec, AvaterAttribute.RotSpeed);
		return _towards;
	}
	*/
}