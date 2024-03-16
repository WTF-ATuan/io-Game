using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigGunUltSkill : UltSkill
{

    private Weapon Weapon;
    public BigGunUltSkill(Weapon weapon) {
        Weapon = weapon;
        RangePreview = new RangePreviewData(weapon.RangePreview.Type, weapon.RangePreview.Dis*1.5f, weapon.RangePreview.Width*1.5f);
    }
    protected override void OnShoot(AvaterState data) {
        var bulletData = new BulletData{
            genPos = data.Pos,
            angle = data.Towards,
            flySec = Weapon.AttributeBonus[AttributeType.FlySec],
            flyDis = Weapon.AttributeBonus[AttributeType.FlyDis]*1.5f,
            damage = Weapon.AttributeBonus[AttributeType.Damage]*1.5f,
            playerId = BattleCtrl.GetLocalPlayerID(),
            runesId = "Ult,Penetration"
        };
        BattleCtrl.GetSpawner().SpawnBulletServerRpc(bulletData);
    }
}
