using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Shotgun : Weapon {
    public Shotgun(
        int maxBullet, float powerChargeToFullSec, float damage, float shootCD, RangePreviewData rangePreview) : 
        base(maxBullet, powerChargeToFullSec, damage, shootCD, rangePreview) {
        
    }

    public override void OnShoot(AvaterStateData data) {
        int bulletAmount = 6;
        float angleRange = RangePreview.Width;
        for (int i = 0; i < bulletAmount; i++) {
            float ang = (data.Towards - angleRange / 2) + i*(angleRange / bulletAmount);
            BattleCtrl.GetSpawner().SpawnBulletServerRpc(data.Pos, ang, 0.3f, RangePreview.Dis);
            //var bullet = BulletPool.Get();
            //bullet.Ctrl.Setup(data.Pos, ang, 0.3f, RangePreview.Dis, () => { bullet.Dispose(); });
        }
    }
}
