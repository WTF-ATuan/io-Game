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
	private AvaterStateData2 StateData2{ get; set; }

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
		
		var weapon = weaponFactory.Create<Shotgun>(3, 6, 1000, 0.5f,new RangePreviewData{Type = RangePreviewType.Sector,Dis = 6,Width = 36});
		Loadout.SetWeapon(weapon, out var unload);
		
		///
		StateData2 = new AvaterStateData2(this);
		RangePreview = GetComponentInChildren<RangePreviewCtrl>();
		RangePreview.Init(StateData2);
		
		HealthBar = healthBarPool.Get();
		HealthBar.Ctrl.Setup(Loadout.NowAttribute, StateData2);
		HealthBar.Obj.transform.parent = transform;
		_recycleThings.Add(HealthBar);
	}

	private void Start() {
		if(IsOwner())BattleCtrl.SetLocalPlayer(this); //Todo add condition => if(currentPlayer == ownerPlayer)
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
		if(IsOwner())RangePreview.Setup(Loadout.GetWeaponInfo().RangePreview);
	}

	private void FixedUpdate() {
		StateData2.ClientUpdate();
	}
}