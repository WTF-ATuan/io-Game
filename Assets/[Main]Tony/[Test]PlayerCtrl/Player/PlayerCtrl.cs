using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public class PlayerCtrl : CreatureCtrl{
	private IInput InputCtrl;
	private RangePreviewCtrl RangePreview;
	private IWeaponFactory _weaponFactory;

	[Inject]
	private void Initialization(
		IInput inputCtrl,
		IWeaponFactory weaponFactory,
		IUltSkillFactory ultSkillFactory
	){
		InputCtrl = inputCtrl;
		_weaponFactory = weaponFactory;
	}

	public override void OnNetworkSpawn(){
		base.OnNetworkSpawn();
		var pistolWeapon = new WeaponData(7, 1, 0.15f, 0.5f, 5f
			, new RangePreviewData(RangePreviewType.Straight, 5, 3f));
		var weapon = _weaponFactory.Create<SnipeGun>(pistolWeapon);
		Loadout.SetWeapon(weapon, out _);
		RangePreview = GetComponentInChildren<RangePreviewCtrl>();
		RangePreview.Init(StateCtrl, Loadout);
		StateCtrl.Data.Health = Loadout.NowAttribute.maxHealth;
		StateCtrl.Data.bulletCount = Loadout.NowAttribute.maxBullet;
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

	[ClientRpc]
	public void SetLevelClientRpc(int level){
		if(level < 1) return;
		Loadout.NowAttribute.moveSpeed *= 1.15f * level;
		Loadout.NowAttribute.maxBullet += Loadout.NowAttribute.maxBullet / 3;
	}

	public void SwitchWeapon(Type weaponType){
		if(weaponType == typeof(SnipeGun)){
			var snipeWeapon = new WeaponData(5, 2.5f, 1.5f, 0.2f, 8f,
				new RangePreviewData(RangePreviewType.Straight, 8f, 4f));
			var weapon = _weaponFactory.Create<SnipeGun>(snipeWeapon);
			Loadout.SetWeapon(weapon, out _);
		}

		if(weaponType == typeof(Shotgun)){
			var shotGun = new WeaponData(3, 0.3f, 1f, 0.3f, 3f,
				new RangePreviewData(RangePreviewType.Sector, 3f, 45f));
			var weapon = _weaponFactory.Create<Shotgun>(shotGun);
			Loadout.SetWeapon(weapon, out _);
		}

		StateCtrl.Data.bulletCount = Loadout.NowAttribute.maxBullet;
	}

	public override async void Reload(){
		Loadout.NowAttribute.moveSpeed *= 0.5f;
		await Task.Delay(1500);
		Loadout.NowAttribute.moveSpeed *= 2f;
		StateCtrl.Data.bulletCount = Loadout.NowAttribute.maxBullet;
	}

	public override bool IsController(){
		return IsOwner && IsClient;
	}


	protected override void Update(){
		base.Update();
		if(IsController())
			RangePreview.Update();
		if(Input.GetKeyDown(KeyCode.Q)){
			SwitchWeapon(typeof(Shotgun));
		}

		if(Input.GetKeyDown(KeyCode.E)){
			SwitchWeapon(typeof(SnipeGun));
		}
	}
}