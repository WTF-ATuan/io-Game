using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public interface INetEntity{
	public ulong GetEntityID();
}

public abstract class CreatureCtrl : NetworkBehaviour, IAvaterSync{
	protected IBattleCtrl BattleCtrl;
	protected AvaterAttribute BaseAttribute;
	protected PlayerLoadout Loadout;
	protected List<IDisposable> RecycleThings;
	protected AvaterStateCtrl StateCtrl;
	protected PoolObj<HealthBarCtrl> HealthBar;

	[Inject]
	private void Initialization(
		IAvaterAttributeCtrl avaterAttributeCtrl,
		IBattleCtrl battleCtrl,
		ObjPoolCtrl<HealthBarCtrl> healthBarPool){
		RecycleThings = new List<IDisposable>();
		BattleCtrl = battleCtrl;
		RecycleThings.Add(BattleCtrl.AddCreature(this));
		StateCtrl = new AvaterStateCtrl(this);
		BaseAttribute = avaterAttributeCtrl.GetData();
		Loadout = new PlayerLoadout(BaseAttribute, this);

		HealthBar = healthBarPool.Get();
		HealthBar.Ctrl.Setup(Loadout, StateCtrl);
		RecycleThings.Add(HealthBar);
	}

	protected NetworkVariable<AvaterState> AvaterSyncData = new();

	public NetworkVariable<AvaterState> GetSyncData(){
		return AvaterSyncData;
	}

	[ServerRpc(RequireOwnership = false)]
	public void AvaterDataSyncServerRpc(AvaterState data){
		AvaterSyncData.Value = data;
	}

	[ClientRpc]
	public void DeathClientRpc(){
		foreach(var thing in RecycleThings){
			thing.Dispose();
		}

		Debug.Log($"player:{OwnerClientId} with {NetworkObjectId} Dead ");
	}

	[ClientRpc]
	public void SetHealthClientRpc(float value){
		StateCtrl.Data.Health = value;
		StateCtrl.DataSync();
	}

	public virtual bool IsController(){
		return false;
	}

	public virtual void Reload(){ }

	public PlayerLoadout GetLoadOut(){
		return Loadout;
	}

	public Transform GetTransform(){
		return transform;
	}

	public abstract IInput GetInput();

	protected virtual void Update(){
		StateCtrl.DataSync();
	}

	protected virtual void FixedUpdate(){
		StateCtrl.ClientUpdate();
	}

	public ulong GetEntityID(){
		return NetworkObjectId;
	}
}