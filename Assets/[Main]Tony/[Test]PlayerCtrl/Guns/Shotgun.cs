using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class Shotgun : Weapon {
    public Shotgun(
        int maxBullet, float powerChargeToFullSec, float damage, float shootCD, float flySec, RangePreviewData rangePreview) : 
        base(maxBullet, powerChargeToFullSec, damage, shootCD, flySec, rangePreview.Dis,rangePreview.Dis,rangePreview) {
    }

    public override void OnShoot(AvaterState data) {

        
        int bulletAmount = 6;
        float angleRange = RangePreview.Width;
        for (int i = 0; i < bulletAmount; i++) {
            float ang = (data.Towards - angleRange / 2) + i*(angleRange / bulletAmount);
            var bulletData = new BulletData{
                genPos = data.Pos,
                angle = data.Towards,
                flySec = AttributeBonus[AttributeType.FlySec],
                flyDis = AttributeBonus[AttributeType.FlyDis],
                damage = AttributeBonus[AttributeType.Damage],
                playerId = BattleCtrl.GetLocalPlayerID(),
                runesId = "Shotgun"
            };
            BattleCtrl.GetSpawner().SpawnBulletServerRpc(bulletData);
            //var bullet = BulletPool.Get();
            //bullet.Ctrl.Setup(data.Pos, ang, 0.3f, RangePreview.Dis, () => { bullet.Dispose(); });
        }
    }
}
