using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class Shotgun : Weapon {
    public Shotgun(WeaponData weaponData) : base(weaponData){ }

    public override async Task OnShoot(AvaterState data) {
        const int bulletAmount = 6;
        var angleRange = RangePreview.Width;
        for (var i = 0; i < bulletAmount; i++) {
            var ang = (data.Towards - angleRange / 2) + i*(angleRange / bulletAmount);
            var bulletData = GetDefaultBullet(data);
            bulletData.angle = ang;
            BattleCtrl.GetSpawner().SpawnBulletServerRpc(bulletData);
        }
    }
}
