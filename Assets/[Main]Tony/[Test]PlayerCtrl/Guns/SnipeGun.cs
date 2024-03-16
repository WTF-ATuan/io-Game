using Unity.Netcode;

public class SnipeGun : Weapon{
	public SnipeGun(int maxBullet, float powerChargeToFullSec, float damage, float shootCD, float flySec,
		RangePreviewData rangePreview) : base(maxBullet, powerChargeToFullSec, damage, shootCD, flySec, rangePreview.Dis,rangePreview){ }

	public override void OnShoot(AvaterState data){
		//var bullet = BulletPool.Get();
		//bullet.Ctrl.Setup(data.Pos, data.Towards, 0.3f, RangePreview.Dis, () => {bullet.Dispose(); });
		var bulletData = new BulletData{
			genPos = data.Pos,
			angle = data.Towards,
			flySec = AttributeBonus[AttributeType.FlySec],
			flyDis = AttributeBonus[AttributeType.FlyDis],
			damage = AttributeBonus[AttributeType.Damage],
			playerId = BattleCtrl.GetLocalPlayerID(),
			runesId = "Frozen"
		};
		BattleCtrl.GetSpawner()
				.SpawnBulletServerRpc(bulletData);
	}
}