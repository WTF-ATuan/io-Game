using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zenject;


public class PlayerCtrl : MonoBehaviour{
	public Transform body;

	private IInput InputCtrl{ get; set; }
	private AvaterAttribute BaseAttribute{ get; set; }
	private AvaterStateData StateData{ get; set; }
	private PlayerLoadout Loadout{ get; set; }
	private PoolObj<HealthBarCtrl> HealthBar{ get; set; }
    private RangePreviewCtrl RangePreview;

    private List<IDisposable> RecycleThings;

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
        RangePreview = GetComponentInChildren<RangePreviewCtrl>();

        Weapon weapon = new Weapon(3,6,1000, 0.5f,new RangePreviewData{Radius = 1,SectorAngle = 0.1f});
        Loadout.SetWeapon(weapon,out List<Item> unload);
    }

		StateData = new AvaterStateData(InputCtrl, Loadout, transform, BaseAttribute);
		HealthBar = healthBarPool.Get();
		HealthBar.Ctrl.Setup(Loadout.NowAttribute, StateData);
		HealthBar.Obj.transform.parent = transform;
		_recycleThings.Add(HealthBar);

		var weapon = new Weapon(3, 6, 1000, 0.5f);
		Loadout.SetWeapon(weapon, out var unload);
	}

	private void OnDestroy(){
		foreach(var thing in _recycleThings){
			thing.Dispose();
		}
	}

	private void FixedUpdate(){
		UpdateActionRequestServerRPC();
	}

	[ServerRpc]
	private void UpdateActionRequestServerRPC(){
		StateData.ClientDataRefresh();
		StateData.LocalUpdate();
		transform.position = StateData.Pos;
		body.eulerAngles = new Vector3(0, 0, StateData.Towards);
	}
    void UpdateAction() {
        StateData.ClientDataRefresh();
        StateData.LocalUpdate();
        transform.position = StateData.Pos;
        Body.eulerAngles = new Vector3(0,0,StateData.Towards);

        if (StateData.IsAim) {
            RangePreview.Setup(Loadout.GetWeaponInfo(out Item[] i).RangePreview,StateData.AimPos.Angle());
        } else
            RangePreview.Setup();
    }
    
    //TODO 能量子彈充能&UI生成

	//TODO 能量子彈充能&UI生成
}