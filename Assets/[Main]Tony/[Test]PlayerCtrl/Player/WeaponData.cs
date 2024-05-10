using System;

public class WeaponData{
	public RangePreviewData RangePreview;
	public float FlyDis;
	public float FlySec;
	public float ShootCd;
	public float Damage;
	public int MaxBullet;
	
	public float AimSlow = 0.5f; 
	public float ShootingDelay = 0.2f;
	public Type WeaponType = typeof(SnipeGun);

	public WeaponData(int maxBullet, float damage, float shootCd, float flySec, float flyDis,
		RangePreviewData rangePreview){
		MaxBullet = maxBullet;
		Damage = damage;
		ShootCd = shootCd;
		FlySec = flySec;
		FlyDis = flyDis;
		RangePreview = rangePreview;
	}
}