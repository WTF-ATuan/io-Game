﻿using System;
using UniRx;
using UniRx.Triggers;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace _Main_Tony._Test_PlayerCtrl.Area{
	public class WeaponArea : NetworkBehaviour{
		private Collider _detectRange;
		[Inject] private IBattleCtrl _battleCtrl;
		private ulong _targetCreatureID = ulong.MaxValue;
		private WeaponType _weaponType;

		public void RandomWeaponType(){
			var values = Enum.GetValues(typeof(WeaponType));
			_weaponType = (WeaponType)values.GetValue(Random.Range(0, values.Length));
		}

		[ClientRpc]
		private void ChangeAreaViewClientRpc(){
			var meshRenderer = GetComponentInChildren<MeshRenderer>();
			var color = _weaponType switch{
				WeaponType.Pistol => Color.green,
				WeaponType.Snipe => Color.yellow,
				WeaponType.ShotGun => Color.red,
				_ => throw new ArgumentOutOfRangeException()
			};
			meshRenderer.material.color = color;
		}

		public override void OnNetworkSpawn(){
			if(!IsServer) return;
			_detectRange = GetComponentInChildren<Collider>();
			_detectRange.OnTriggerEnterAsObservable().Subscribe(OnCreatureEnter);
			RandomWeaponType();
			ChangeAreaViewClientRpc();
		}

		private void OnCreatureEnter(Collider obj){
			var creature = obj.transform.parent.GetComponent<CreatureCtrl>();
			if(!creature) return;
			_targetCreatureID = creature.GetEntityID();
			_battleCtrl.SwitchWeaponServerRpc(_targetCreatureID , _weaponType);
		}
	}

	public enum WeaponType{
		Pistol,
		Snipe,
		ShotGun,
	}
}