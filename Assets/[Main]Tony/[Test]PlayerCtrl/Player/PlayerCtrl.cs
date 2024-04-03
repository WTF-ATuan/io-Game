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
	){
		InputCtrl = inputCtrl;
		var weapon = weaponFactory.Create<SnipeGun>(3, 6, 1000, 0.5f, 0.3f, 0.2f,
			new RangePreviewData(RangePreviewType.Straight, 6, 10));
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

	public override IInput GetInput(){
		return InputCtrl;
	}

	[ClientRpc]
	public void SetHealthClientRpc(float value){
		StateCtrl.Data.Health = value;
		StateCtrl.DataSync();
	}

	[ClientRpc]
	public void DeathClientRpc(){
		Debug.Log($"You are Dead!");
	}

	[ClientRpc]
	public void ForceToClientRpc(Vector2 forceCenter){
		var avaterData = StateCtrl.Data;
		var currentVec = avaterData.NowVec;

		if(currentVec.magnitude < Mathf.Epsilon){
			var lerpForce = Mathf.Clamp01((forceCenter - avaterData.Pos).magnitude);
			avaterData.NowVec = (forceCenter - avaterData.Pos).normalized * lerpForce;
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