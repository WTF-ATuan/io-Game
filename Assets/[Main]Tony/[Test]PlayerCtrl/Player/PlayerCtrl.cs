using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public class PlayerCtrl : NetworkBehaviour,IAvaterSync{
	
	private IBattleCtrl BattleCtrl;
	private IInput InputCtrl;
	private AvaterAttribute BaseAttribute;
	private PlayerLoadout Loadout;
	private PoolObj<HealthBarCtrl> HealthBar;
	private List<IDisposable> _recycleThings;
	private RangePreviewCtrl RangePreview;
	private AvaterStateCtrl StateCtrl;

	[Inject]
	private void Initialization(
		IAvaterAttributeCtrl avaterAttributeCtrl,
		IInput inputCtrl,
		IBattleCtrl battleCtrl,
		ObjPoolCtrl<HealthBarCtrl> healthBarPool,
		IWeaponFactory weaponFactory,
		IUltSkillFactory ultSkillFactory
	) {
		BattleCtrl = battleCtrl;
		InputCtrl = inputCtrl;
		_recycleThings = new List<IDisposable>();

		StateCtrl = new AvaterStateCtrl(this);
		RangePreview = GetComponentInChildren<RangePreviewCtrl>();
		RangePreview.Init(StateCtrl);
		
		BaseAttribute = avaterAttributeCtrl.GetData();
		Loadout = new PlayerLoadout(BaseAttribute);
		var weapon = weaponFactory.Create<SnipeGun>(3, 6, 1000, 0.5f,new RangePreviewData{Type = RangePreviewType.Straight,Dis = 6,Width = 10});
		Loadout.SetWeapon(weapon, out var unload);

		//var ultSkill = ultSkillFactory.Create<UltSkill>();
		//Loadout.SetUltSkill(ultSkill, out var unload2);
		
		HealthBar = healthBarPool.Get();
		HealthBar.Ctrl.Setup(Loadout.NowAttribute, StateCtrl);
		HealthBar.Obj.transform.SetParent(transform);
		_recycleThings.Add(HealthBar);
	}

	public override void OnNetworkSpawn() {
		if(IsOwner())BattleCtrl.SetLocalPlayer(this);
	}
	
	public override void OnDestroy(){
		base.OnDestroy();
		foreach(var thing in _recycleThings){
			thing.Dispose();
		}
	}
	
	private NetworkVariable<AvaterState> AvaterSyncData = new();
	public NetworkVariable<AvaterState> GetSyncData() {
		return AvaterSyncData;
	}
	
	[ServerRpc(RequireOwnership = false)]
	public void AvaterDataSyncServerRpc(AvaterState data) {
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
	
	public void ModifyHealth(int amount){
		StateCtrl.ModifyHealth(amount);
	}

	public bool IsOwner() {
		return base.IsOwner && base.IsClient;
	}
	
	private void Update() {
		StateCtrl.DataSync();
		if(IsOwner())RangePreview.Setup(Loadout.GetWeaponInfo().RangePreview);
	}

	private void FixedUpdate() {
		StateCtrl.ClientUpdate();
	}
}