using System.Threading.Tasks;
using UnityEngine;

public class SnipeGun : Weapon{
	public SnipeGun(WeaponData weaponData) : base(weaponData){ }

	public override async Task OnShoot(AvaterState data){
		IsPauseAim = true;
		await Task.Delay(Mathf.RoundToInt(weaponData.ShootingDelay * 1000));
		BattleCtrl.GetSpawner().SpawnBulletServerRpc(GetDefaultBullet(data));
		IsPauseAim = false;
	}
}