using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class Shotgun : Weapon {
    
    public Shotgun(int maxBullet, float powerChargeToFullSec, float damage, float shootCD, float flySec, float flyDis, RangePreviewData rangePreview) : 
        base(maxBullet, powerChargeToFullSec, damage, shootCD, flySec, flyDis, rangePreview) { }
    
    public override async Task OnShoot(AvaterState data) {
        int bulletAmount = 6;
        float angleRange = RangePreview.Width;
        for (int i = 0; i < bulletAmount; i++) {
            float ang = (data.Towards - angleRange / 2) + i*(angleRange / bulletAmount);
            var bulletData = GetDefaultBullet(data);
            bulletData.angle = ang;
            BattleCtrl.GetSpawner().SpawnBulletServerRpc(bulletData);
        }
    }


}
