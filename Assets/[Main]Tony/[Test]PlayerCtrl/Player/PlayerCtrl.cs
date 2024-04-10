using Unity.Netcode;
using UnityEngine;
using Zenject;

public class PlayerCtrl : CreatureCtrl{
	private IInput InputCtrl;
	private RangePreviewCtrl RangePreview;

	[Inject]
	private void Initialization(
		IInput inputCtrl,
		IWeaponFactory weaponFactory,
		IUltSkillFactory ultSkillFactory
	){
		InputCtrl = inputCtrl;
		var weapon = weaponFactory.Create<SnipeGun>(5, 6, 1000, 0.5f, 0.3f, 6f, new RangePreviewData(RangePreviewType.Throw, 6, 2f));
		//var weapon = weaponFactory.Create<OlaOlaGun>(3, 6, 1000, 0.5f,0.3f,1f,new RangePreviewData(RangePreviewType.Straight,1,10));
		Loadout.SetWeapon(weapon, out var unload);
		//var bigGunUlt = ultSkillFactory.Create<BigGunUltSkill>(weapon);
		//Loadout.SetUltSkill(bigGunUlt, out var unloadUlt);
		var bearUlt = ultSkillFactory.Create<BearUltSkill>();
		Loadout.SetUltSkill(bearUlt, out var unloadUlt);

		RangePreview = GetComponentInChildren<RangePreviewCtrl>();
		RangePreview.Init(StateCtrl, Loadout);


	}

	public override IInput GetInput(){
		return InputCtrl;
	}

	[ClientRpc]
	public void ForceToClientRpc(Vector2 forceCenter){
		var avaterData = StateCtrl.Data;
		var currentVec = avaterData.NowVec;

		if(currentVec.magnitude < Mathf.Epsilon){
			var lerpForce = Mathf.Clamp01((forceCenter - avaterData.Pos).magnitude);
			avaterData.NowVec = (forceCenter - avaterData.Pos).normalized * (lerpForce * 0.5f);
			avaterData.Pos += avaterData.NowVec * (Time.time - avaterData.ClientUpdateTimeStamp);
			StateCtrl.DataSync();
		}
		else{
			var forceDirection = (avaterData.Pos - forceCenter).normalized;
			var lerpFactor = Vector2.Dot(forceDirection, currentVec.normalized);
			var moveSpeed = Mathf.Lerp(avaterData.NowVec.magnitude, avaterData.NowVec.magnitude * 1.5f, lerpFactor);
			avaterData.NowVec = Vector2.Lerp(avaterData.NowVec, currentVec.normalized * moveSpeed, Time.deltaTime);
			avaterData.Pos += avaterData.NowVec * (Time.time - avaterData.ClientUpdateTimeStamp);
			StateCtrl.DataSync();
		}
	}

	public override bool IsController(){
		return IsOwner && IsClient;
	}


	protected override void Update(){
		base.Update();
		if(IsController())
			RangePreview.Update();
	}
}