using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class OlaOlaGun : Weapon {

    public OlaOlaGun(int maxBullet, float powerChargeToFullSec, float damage, float shootCD, float flySec, float flyDis, RangePreviewData rangePreview) : base(maxBullet, powerChargeToFullSec, damage, shootCD, flySec, flyDis, rangePreview)
    {
    }

    public override async Task OnShoot(AvaterState data)
    {
        IsPauseAim = true;
        for (int i = 0; i < 5; i++) {
            await Task.Delay(100);
            BattleCtrl.GetSpawner().SpawnBulletServerRpc(GetDefaultBullet(data));
        }
        IsPauseAim = false;
    }


}
