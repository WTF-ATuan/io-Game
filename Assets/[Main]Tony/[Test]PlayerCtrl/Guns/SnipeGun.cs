using System.Threading.Tasks;

public class SnipeGun : Weapon{
	public SnipeGun(WeaponData weaponData) : base(weaponData){ }

	public override async Task OnShoot(AvaterState data){
		IsPauseAim = true;
		await Task.Delay(200);
		BattleCtrl.GetSpawner().SpawnBulletServerRpc(GetDefaultBullet(data));
		IsPauseAim = false;
	}

}