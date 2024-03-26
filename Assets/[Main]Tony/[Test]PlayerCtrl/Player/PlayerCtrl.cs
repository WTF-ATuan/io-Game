using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using Zenject;

public class PlayerCtrl : CreatureCtrl{
	
	private IInput InputCtrl;
	private PoolObj<HealthBarCtrl> HealthBar;
	private RangePreviewCtrl RangePreview;
	
	[Inject]
	private void Initialization(
		IInput inputCtrl,
		ObjPoolCtrl<HealthBarCtrl> healthBarPool,
		IWeaponFactory weaponFactory,
		IUltSkillFactory ultSkillFactory
	) {
		InputCtrl = inputCtrl;
		var weapon = weaponFactory.Create<SnipeGun>(3, 6, 1000, 0.5f,0.3f,0.2f,new RangePreviewData(RangePreviewType.Straight,6,10));
		Loadout.SetWeapon(weapon, out var unload);
		var bigGunUlt = ultSkillFactory.Create<BigGunUltSkill>(weapon);
		Loadout.SetUltSkill(bigGunUlt, out var unloadUlt);

		RangePreview = GetComponentInChildren<RangePreviewCtrl>();
		RangePreview.Init(StateCtrl, Loadout);
		
		HealthBar = healthBarPool.Get();
		HealthBar.Ctrl.Setup(Loadout.NowAttribute, StateCtrl);
		HealthBar.Obj.transform.SetParent(transform);
		RecycleThings.Add(HealthBar);
	}

	public override IInput GetInput() {
		return InputCtrl;
	}
	
	[ClientRpc]
	public void ModifyHealthClientRpc(int amount){
		StateCtrl.ModifyHealth(amount);
	}

	public override bool IsController() {
		return IsOwner && IsClient;
	}


	protected override void Update() {
		base.Update();
		if(IsController())
			RangePreview.Update();
	}
}