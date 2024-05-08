using System.Threading.Tasks;
using UnityEngine;

public class SnipeGun : Weapon{
	private bool _oneTimeFlag;
	private Vector2 _originVec;
	public SnipeGun(WeaponData weaponData) : base(weaponData){ }

	public override async Task OnShoot(AvaterState data){
		IsPauseAim = true;
		await Task.Delay(Mathf.RoundToInt(WeaponData.ShootingDelay * 1000));
		BattleCtrl.GetSpawner().SpawnBulletServerRpc(GetDefaultBullet(data));
		IsPauseAim = false;
		data.NowVec = _originVec;
		_oneTimeFlag = false;
	}

	public override bool TryShoot(AvaterState data, bool forceShoot = true){
		var tryShoot = base.TryShoot(data, forceShoot);
		if(tryShoot){
			if(!_oneTimeFlag){
				_originVec = data.NowVec;
				_oneTimeFlag = true;
			}

			data.NowVec = _originVec * WeaponData.AimSlow;
		}

		return tryShoot;
	}
}