using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnipeGun : Weapon {
    public SnipeGun(int maxBullet, float powerChargeToFullSec, float damage, float shootCD, RangePreviewData rangePreview) : base(maxBullet, powerChargeToFullSec, damage, shootCD, rangePreview) {
    }

    public override void OnShoot(AvaterStateData data) {
        var bullet = BulletPool.Get();
        bullet.Ctrl.Setup(data.Pos, data.Towards, 0.3f, 8, () => {bullet.Dispose(); });
    }


}
