using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public class PlayerCtrl : NetworkBehaviour,IAvaterSync{
	public Transform body;

	private IBattleCtrl BattleCtrl;
	private IInput InputCtrl{ get; set; }
	private AvaterAttribute BaseAttribute{ get; set; }
	private PlayerLoadout Loadout{ get; set; }
	private PoolObj<HealthBarCtrl> HealthBar{ get; set; }

	private List<IDisposable> _recycleThings;
	private RangePreviewCtrl RangePreview;
	private AvaterStateCtrl StateCtrl{ get; set; }

	[Inject]
	private void Initialization(
		IAvaterAttributeCtrl avaterAttributeCtrl,
		IInput inputCtrl,
		IBattleCtrl battleCtrl,
		ObjPoolCtrl<HealthBarCtrl> healthBarPool,
		IWeaponFactory weaponFactory
	)
	{
		BattleCtrl = battleCtrl;
		_recycleThings = new List<IDisposable>();
		InputCtrl = inputCtrl;
		BaseAttribute = avaterAttributeCtrl.GetData();
		Loadout = new PlayerLoadout(BaseAttribute);
		
		var weapon = weaponFactory.Create<SnipeGun>(3, 6, 1000, 0.5f,new RangePreviewData{Type = RangePreviewType.Straight,Dis = 6,Width = 10});
		Loadout.SetWeapon(weapon, out var unload);
		
		///
		StateCtrl = new AvaterStateCtrl(this);
		RangePreview = GetComponentInChildren<RangePreviewCtrl>();
		RangePreview.Init(StateCtrl);
		
		HealthBar = healthBarPool.Get();
		HealthBar.Ctrl.Setup(Loadout.NowAttribute, StateCtrl);
		HealthBar.Obj.transform.parent = transform;
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