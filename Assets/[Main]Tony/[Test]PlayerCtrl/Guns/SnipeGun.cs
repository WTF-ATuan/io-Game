using System;
using System.Threading.Tasks;
using Unity.Netcode;

public class SnipeGun : Weapon{

	public SnipeGun(int maxBullet, float powerChargeToFullSec, float damage, float shootCD, float flySec, float flyDis, RangePreviewData rangePreview) : 
		base(maxBullet, powerChargeToFullSec, damage, shootCD, flySec, flyDis, rangePreview) { }
	
	public override async Task OnShoot(AvaterState data){
		IsPauseAim = true;
		await Task.Delay(200);
		BattleCtrl.GetSpawner().SpawnBulletServerRpc(GetDefaultBullet(data));
		IsPauseAim = false;
	}
	
}