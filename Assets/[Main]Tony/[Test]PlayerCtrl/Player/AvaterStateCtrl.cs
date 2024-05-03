using UnityEngine;


public class AvaterStateCtrl : INetEntity{
	private IAvaterSync Avater;
	public Transform RotCenter{ get; private set; }

	public AvaterState Data{ get; private set; }
	private NetworkValue.Vec2Smoother PosSmoother;
	private NetworkValue.RotSmoother RotSmoother;
	private bool IsInit = false;

	public AvaterStateCtrl(IAvaterSync avater){
		Avater = avater;
		RotCenter = Avater.GetTransform().Find("RotCenter");
		Data = new AvaterState();

		if(!Avater.IsController()){
			PosSmoother = new NetworkValue.Vec2Smoother(() => Data.Pos, () => Avater.GetTransform().position);
			RotSmoother = new NetworkValue.RotSmoother(() => Data.Towards, () => RotCenter.eulerAngles.z);
			Avater.GetSyncData().OnValueChanged += (value, newValue) => {
				PosSmoother.Update();
				RotSmoother.Update();
			};
		}

		Data.Pos = Avater.GetTransform().position;
	}

	public void DataSync(){
		if(Avater.IsController()){
			Avater.AvaterDataSyncServerRpc(Data);
			Reload();
		}
	}

	public void ClientUpdate(){
		if(Avater.IsController()){
			var missTime = Time.time - Data.ClientUpdateTimeStamp;
			Data.ClientUpdateTimeStamp = Time.time;
			UpdateInput();
			UpdateMove(missTime);
			UpdateRotate(missTime);
			Shoot(missTime);


			Avater.GetTransform().position = Data.Pos;
			RotCenter.eulerAngles = Vector3.forward * Data.Towards;
		}
		else{
			Data = Avater.GetSyncData().Value;

			Avater.GetTransform().position = PosSmoother.Get();
			RotCenter.eulerAngles = Vector3.forward * RotSmoother.Get();
		}
	}

	private void Reload(){
		if(Avater.GetInput().Reload()){
			Avater.Reload();
		}
	}

	private void Shoot(float missTime){
		var weapon = Avater.GetLoadOut().GetWeaponInfo();
		var isEmpty = Data.bulletCount < 1;
		if(isEmpty) return;
		if(weapon != null && weapon.TryShoot(Data)){
			Data.Towards = Data.LastAimPos.Angle();
			Data.RotVec = 0;
			Data.bulletCount--;
		}
	}

	private void UpdateRotate(float missTime){
		var weapon = Avater.GetLoadOut().GetWeaponInfo();
		var ultSkill = Avater.GetLoadOut().GetUtlInfo();
		float targetTowards =
				weapon.IsPauseAim ? Data.Towards :
				Data.IsAim && weapon.TryShoot(Data, false) ? Data.AimPos.Angle() :
				Data.IsUtl && ultSkill.TryShoot(Data, false) ? Data.UtlPos.Angle() :
				Data.TargetVec != Vector2.zero ? Data.TargetVec.Angle() :
				Data.Towards;
		Data.Towards = Mathf.SmoothDampAngle(Data.Towards, targetTowards, ref Data.RotVec, AvaterAttribute.RotSpeed,
			Mathf.Infinity, missTime);
	}

	private void UpdateMove(float missTime){
		var weapon = Avater.GetLoadOut().GetWeaponInfo();
		if(weapon.IsPauseMove) Data.TargetVec = Vector2.zero;
		var vec = Data.TargetVec - Data.NowVec;
		var direction = vec.normalized;
		var distance = vec.magnitude;
		var moveFriction = AvaterAttribute.MoveFriction;

		Data.NowVec = distance > moveFriction
				? Data.NowVec + direction * Mathf.Min(moveFriction, distance)
				: Data.TargetVec;
		Data.Pos += Data.NowVec * (Avater.GetLoadOut().NowAttribute.moveSpeed * missTime);
	}

	private void UpdateInput(){
		Data.TargetVec = Avater.GetInput().MoveJoy();
		Data.LastAimPos = Data.AimPos;
		Data.AimPos = Avater.GetInput().AimJoy();
		Data.LastUtlPos = Data.UtlPos;
		Data.UtlPos = Avater.GetInput().UtlJoy();
	}

	public ulong GetEntityID(){
		return Avater.GetEntityID();
	}
}