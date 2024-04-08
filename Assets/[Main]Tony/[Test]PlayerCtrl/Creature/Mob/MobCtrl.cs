using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public class MobCtrl : CreatureCtrl
{
    private ServerInput Input;
    private Transform Target;

    [Inject]
    private void Initialization(
        IAvaterAttributeCtrl avaterAttributeCtrl,
        IWeaponFactory weaponFactory) {
        BaseAttribute = avaterAttributeCtrl.GetData(1);
        Loadout = new PlayerLoadout(BaseAttribute, this);
        Input = new ServerInput();
        var weapon = weaponFactory.Create<SnipeGun>(3, 6, 1000, 0.5f,0.3f,0.1f,new RangePreviewData(RangePreviewType.Straight,6,10));
        Loadout.SetWeapon(weapon, out var unload);
    }
    
    public override void OnNetworkSpawn()
    {
        async Task RepeatActionEvery(TimeSpan interval, Func<Task> action) {
            while (true) {
                await Task.Delay(interval);
                await action();
            }
        }
        
        if (IsServer) {
            RepeatActionEvery(TimeSpan.FromSeconds(0.1f), async () => {
                var list = BattleCtrl.GetCreatureList();
                var minDis = 0f;
                CreatureCtrl target = null;
                foreach (var creature in list) {
                    if (creature == this) continue;
                    var dis = (transform.position - creature.transform.position).magnitude;
                    if (target == null || dis < minDis) {
                        minDis = dis;
                        target = creature;
                    }
                }

                if(target!=null)Target = target.transform;

                Vector2 moveJoy = Vector2.zero;
                Vector2 aimJoy = Vector2.zero;
                if (Target != null) {
                    var aimVec = Vector3.zero;
                    var moveVec = Vector3.zero;
                    
                    if (AStarCtrl.AStar(transform.position.ToVec2Int(),Target.position.ToVec2Int(),
                        BattleCtrl.GetGroundList().Cast<IAStarGround>().ToList(), out Stack<IAStarGround> path)) {
                        if(transform.position.ToVec2Int()==path.Peek().GetPos())path.Pop();
                        if (path.Count > 0) {
                            moveVec = ((Vector3) (Vector2) path.Peek().GetPos()) - transform.position;
                            moveJoy = moveVec.magnitude>1?moveVec.normalized:moveVec;
                        }
                        aimVec = (Target.position+(Vector3)target.GetInput().MoveJoy()*1.5f) - transform.position;
                        if (aimVec.magnitude < Loadout.GetWeaponInfo().AttributeBonus[AttributeType.FlyDis]) {
                            aimJoy = Loadout.GetWeaponInfo().TryShoot(StateCtrl.Data, false) && Input._AimJoy==Vector2.zero ? aimVec.normalized : Vector2.zero;
                        }
                    }
                }
                Input._MoveJoy = moveJoy;
                Input._AimJoy = aimJoy;
            });
        }
    }

    public override IInput GetInput() {
        return Input;
    }
    
    public override bool IsController() {
        return IsServer;
    }
}
