using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigGunUltSkill : UltSkill {
    protected override void OnShoot(AvaterState data)
    {
        var bulletData = new BulletData{
            genPos = data.Pos,
            angle = data.Towards,
            moveSec = 0.3f,
            maxDis =  7.5f,
            damage = 175,
            playerId = BattleCtrl.GetLocalPlayerID(),
            runesId = "Ult,Penetration"
        };
        BattleCtrl.GetSpawner()
                .SpawnBulletServerRpc(bulletData);
    }
}
