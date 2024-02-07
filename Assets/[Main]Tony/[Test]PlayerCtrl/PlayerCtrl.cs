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

	[Inject]
	private void Initialization(
		IAvaterAttributeCtrl avaterAttributeCtrl,
		IInput inputCtrl,
		IBattleCtrl battleCtrl,
		ObjPoolCtrl<HealthBarCtrl> healthBarPool){
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

		var weapon = new Weapon(3, 6, 1000, 0.5f);
		Loadout.SetWeapon(weapon, out var unload);
	}

	public override void OnDestroy(){
		base.OnDestroy();
		foreach(var thing in _recycleThings){
			thing.Dispose();
		}
	}

	private void FixedUpdate(){
		if(!IsOwner) return;
		CalculateActionData();
	}

	private void CalculateActionData(){
		StateData.ClientDataRefresh();
		StateData.LocalUpdate();
		UpdateActionRequestServerRPC();
	}

	[ServerRpc(RequireOwnership = false)]
	private void UpdateActionRequestServerRPC(){
		transform.position = StateData.Pos;
		body.eulerAngles = new Vector3(0, 0, StateData.Towards);
	}

	//TODO 能量子彈充能&UI生成
}