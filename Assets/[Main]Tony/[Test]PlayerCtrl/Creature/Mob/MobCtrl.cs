using System;
using System.Collections;
using System.Collections.Generic;
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
        Loadout = new PlayerLoadout(BaseAttribute);
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
                    var vec = (Target.position+(Vector3)target.GetInput().MoveJoy()*1.5f) - transform.position;
                    moveJoy = vec.magnitude>1?vec.normalized:vec;
                    aimJoy = Loadout.GetWeaponInfo().TryShoot(StateCtrl.Data, false) && Input._AimJoy==Vector2.zero ? vec.normalized : Vector2.zero;
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
