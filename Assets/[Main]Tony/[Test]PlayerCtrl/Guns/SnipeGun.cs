using System.Threading.Tasks;
using UnityEngine;

public class SnipeGun : Weapon{
	public SnipeGun(WeaponData weaponData) : base(weaponData){ }

	public override async Task OnShoot(AvaterState data){
		IsPauseAim = true;
		await Task.Delay(Mathf.RoundToInt(WeaponData.ShootingDelay * 1000));
		BattleCtrl.GetSpawner().SpawnBulletServerRpc(GetDefaultBullet(data));
		IsPauseAim = false;
	}

	public override bool TryShoot(AvaterState data, bool forceShoot = true){
		var tryShoot = base.TryShoot(data, forceShoot);
		if(tryShoot){
			if(data.NowVec.magnitude > 0.1f){
				data.NowVec *= WeaponData.AimSlow;
			}
			
		}

		return tryShoot;
	}
}