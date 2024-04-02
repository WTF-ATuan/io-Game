using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace _Main_Tony._Test_PlayerCtrl.Area{
	public class Area : MonoBehaviour{
		private Collider _detectRange;
		private ulong _ownerID = ulong.MaxValue;
		[Inject] private IBattleCtrl _battleCtrl;
		[SerializeField] private float duration = 2f;

		private ulong _targetCreatureID = ulong.MaxValue;
		private float _timer = 0;


		public void SetOwner(ulong clientID){
			_ownerID = clientID;
		}

		private void Start(){
			_detectRange = GetComponentInChildren<Collider>();
			_detectRange.OnTriggerEnterAsObservable().Subscribe(OnCreatureEnter);
			_detectRange.OnTriggerExitAsObservable().Subscribe(OnCreatureExit);
		}

		private void OnCreatureExit(Collider obj){
			var creature = obj.transform.parent.GetComponent<CreatureCtrl>();
			if(!creature) return;
			_targetCreatureID = ulong.MaxValue;
			_timer = duration;
		}

		private void OnCreatureEnter(Collider obj){
			var creature = obj.transform.parent.GetComponent<CreatureCtrl>();
			if(!creature) return;
			_targetCreatureID = creature.OwnerClientId;
		}

		private void FixedUpdate(){
			if(_targetCreatureID >= ulong.MaxValue){
				return;
			}

			_timer += Time.fixedDeltaTime;
			if(_timer < duration) return;
			_battleCtrl.PlayerHitRequestServerRpc(_ownerID, _targetCreatureID, 50);
			_timer = 0;
		}
	}
}