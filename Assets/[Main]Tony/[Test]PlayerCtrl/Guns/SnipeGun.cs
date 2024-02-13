using Unity.Netcode;

public class SnipeGun : Weapon {
    public SnipeGun(int maxBullet, float powerChargeToFullSec, float damage, float shootCD, RangePreviewData rangePreview) : base(maxBullet, powerChargeToFullSec, damage, shootCD, rangePreview) {
    }

    public override void OnShoot(AvaterState data) {
        //var bullet = BulletPool.Get();
        //bullet.Ctrl.Setup(data.Pos, data.Towards, 0.3f, RangePreview.Dis, () => {bullet.Dispose(); });
        BattleCtrl.GetSpawner().SpawnBulletServerRpc(data.Pos, data.Towards, 0.3f, RangePreview.Dis , NetworkManager.Singleton.LocalClientId);
    }


}
