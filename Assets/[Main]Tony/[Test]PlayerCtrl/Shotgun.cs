using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Shotgun : Weapon {
    public Shotgun(
        int maxBullet, float powerChargeToFullSec, float damage, float shootCD, RangePreviewData rangePreview) : 
        base(maxBullet, powerChargeToFullSec, damage, shootCD, rangePreview) {
        
    }

    public override bool CanShoot(AvaterStateData data)
    {
        float powerNeed = (1f / (int) AttributeBonus[AttributeType.MaxBullet]);
        float nowTime = Time.time;
        if (data.AimPos == Vector2.zero && 
            data.LastAimPos != Vector2.zero && 
            data.ShootCd < nowTime && 
            data.Power >= powerNeed) {
            data.Power = Mathf.Clamp01(data.Power - powerNeed);
            data.ShootCd = nowTime + AttributeBonus[AttributeType.ShootCD];

            int bulletAmount = 6;
            float angleRange = RangePreview.SectorAngle*360f;
            for (int i = 0; i < bulletAmount; i++) {
                var bullet = BulletPool.Get();
                float ang = (data.Towards - angleRange / 2) + i*(angleRange / bulletAmount);
                Debug.Log(ang);
                bullet.Ctrl.Setup(data.Pos, ang, 0.3f, 6, () => {bullet.Dispose(); });
            }

            return true;
        }

        return false;
        
    }

    public override object Clone() {
        return new Shotgun(
            (int) AttributeBonus[AttributeType.MaxBullet],
            AttributeBonus[AttributeType.PowerChargeToFullSec],
            AttributeBonus[AttributeType.Damage],
            AttributeBonus[AttributeType.ShootCD],
            RangePreview);
    }
}
