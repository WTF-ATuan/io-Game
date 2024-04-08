using System;
using System.Threading.Tasks;
using Unity.Netcode;

public class SnipeGun : Weapon{
	public SnipeGun(int maxBullet, float powerChargeToFullSec, float damage, float shootCD, float flySec, float shootDelay,
		RangePreviewData rangePreview) : base(maxBullet, powerChargeToFullSec, damage, shootCD, flySec, rangePreview.Dis, shootDelay, rangePreview){ }

	public override void OnShoot(AvaterState data){
		var bulletData = new BulletData{
			genPos = data.Pos,
			angle = data.Towards,
			flySec = AttributeBonus[AttributeType.FlySec],
			flyDis = AttributeBonus[AttributeType.FlyDis],
			damage = AttributeBonus[AttributeType.Damage],
			playerId = Owner.GetEntityID(),
			runesId = "Frozen"
		};
		BattleCtrl.GetSpawner()
			.SpawnBulletServerRpc(bulletData);
	}
}