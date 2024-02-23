using Unity.Netcode;
using UnityEngine;

namespace _Main_Tony._Test_PlayerCtrl.Runes{
	public class RunesCastHelper{
		private double _serverTime;
		private double _endTimeStamp = double.MaxValue;
		public PlayerCtrl Owner{ get; }

		public RunesCastHelper(PlayerCtrl owner){
			Owner = owner;
		}

		public void CastingRunesEffect(string runesId, RunesCastType castType){
			if(runesId != "FrozenRune") return;
			if(castType != RunesCastType.AutoAttack) return;
			//fake data
			const float modifiedMoveSpeed = 0.5f;
			const float castingTime = 1f;
			_endTimeStamp = NetworkManager.Singleton.ServerTime.Time + castingTime;
			Owner.GetLoadOut().NowAttribute.MoveSpeed *= modifiedMoveSpeed;
			Debug.Log("Cast Frozen Effect");
		}

		public void SyncServerTime(){
			_serverTime = NetworkManager.Singleton.ServerTime.Time;
			if(_serverTime >= _endTimeStamp){
				Owner.GetLoadOut().NowAttribute.MoveSpeed *= 2;
				_endTimeStamp = double.MaxValue;
			}
		}
		
	}
}