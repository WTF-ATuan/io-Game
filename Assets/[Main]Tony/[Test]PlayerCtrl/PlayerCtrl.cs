using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;


public class PlayerCtrl : MonoBehaviour
{
    public Transform Body;
    
    public IInput InputCtrl { get; private set; }
    public AvaterAttribute BaseAttribute { get; private set; }
    public AvaterStateData StateData { get; private set; }
    public PlayerLoadout Loadout{ get; private set; }
    public PoolObj<HealthBarCtrl> HealthBar{ get; private set; }

    private List<IDisposable> RecycleThings;

    [Inject]
    void Constructor(
        IAvaterAttributeCtrl avaterAttributeCtrl, 
        IInput inputCtrl,
        IBattleCtrl battleCtrl,
        ObjPoolCtrl<HealthBarCtrl> healthBarPool)
    {
        battleCtrl.SetLocalPlayer(this);
        RecycleThings = new List<IDisposable>();
        InputCtrl = inputCtrl;
        BaseAttribute = avaterAttributeCtrl.GetData();
        StateData = new AvaterStateData(this);
        Loadout = new PlayerLoadout(BaseAttribute);
        
        HealthBar = healthBarPool.Get();
        HealthBar.Ctrl.Setup(Loadout.NowAttribute, StateData);
        HealthBar.Obj.transform.parent = transform;
        RecycleThings.Add(HealthBar);

        Weapon weapon = new Weapon(3,6,1000);
        Loadout.SetWeapon(weapon,out List<Item> unload);
    }

    private void OnDestroy() {
        foreach (var thing in RecycleThings) {
            thing.Dispose();
        }
    }

    private void Start() { }

    /*
    async Task ServerTest() {
        while (true) {
            ActionData.ServerDataRefresh();
            await Task.Delay(50);
        }
    }
    */

    private void FixedUpdate() {
        UpdateAction();
    }

    void UpdateAction() {
        StateData.ClientDataRefresh();
        StateData.LocalUpdate();
        transform.position = StateData.Pos;
        Body.eulerAngles = new Vector3(0,0,StateData.Towards);
    }
    
    //TODO 能量子彈充能&UI生成

}
