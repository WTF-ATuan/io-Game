using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public abstract class CreatureCtrl : NetworkBehaviour ,IAvaterSync{
    protected IBattleCtrl BattleCtrl;
    protected AvaterAttribute BaseAttribute;
    protected PlayerLoadout Loadout;
    protected List<IDisposable> RecycleThings;
    protected AvaterStateCtrl StateCtrl;
    
    [Inject]
    private void Initialization(
        IAvaterAttributeCtrl avaterAttributeCtrl,
        IBattleCtrl battleCtrl) {
        BattleCtrl = battleCtrl;
        BattleCtrl.AddCreature(this);
        RecycleThings = new List<IDisposable>();
        StateCtrl = new AvaterStateCtrl(this);
        BaseAttribute = avaterAttributeCtrl.GetData();
        Loadout = new PlayerLoadout(BaseAttribute);
    }

    public override void OnDestroy(){
        base.OnDestroy();
        foreach(var thing in RecycleThings){
            thing.Dispose();
        }
    }
    
    protected NetworkVariable<AvaterState> AvaterSyncData = new();
    public NetworkVariable<AvaterState> GetSyncData() {
        return AvaterSyncData;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void AvaterDataSyncServerRpc(AvaterState data) {
        AvaterSyncData.Value = data;
    }

    public virtual bool IsController() {
        return false;
    }

    public PlayerLoadout GetLoadOut() {
        return Loadout;
    }
	
    public Transform GetTransform() {
        return transform;
    }

    public abstract IInput GetInput();
    
    protected virtual void Update() {
        StateCtrl.DataSync();
 
    }

    protected virtual void FixedUpdate() {
        StateCtrl.ClientUpdate();
    }
    
}
