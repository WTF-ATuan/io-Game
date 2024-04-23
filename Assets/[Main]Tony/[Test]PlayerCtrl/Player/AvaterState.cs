using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class AvaterState : INetworkSerializable
{
	public Vector2 Pos;
	public Vector2 TargetVec;
	public Vector2 NowVec;
	public Vector2 AimPos;
	public Vector2 LastAimPos;
	public Vector2 UtlPos;
	public Vector2 LastUtlPos;
	public float Towards;
	public float RotVec;
	public float ClientUpdateTimeStamp;
	public float Power;
	public float ShootCd;
	public float UltPower;
	public float Health;
	public int bulletCount;

	public bool IsAim => AimPos != Vector2.zero;
	public bool IsUtl => UtlPos != Vector2.zero;
	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Pos);
		serializer.SerializeValue(ref TargetVec);
		serializer.SerializeValue(ref NowVec);
		serializer.SerializeValue(ref AimPos);
		serializer.SerializeValue(ref LastAimPos);
		serializer.SerializeValue(ref UtlPos);
		serializer.SerializeValue(ref LastUtlPos);
		serializer.SerializeValue(ref Towards);
		serializer.SerializeValue(ref RotVec);
		serializer.SerializeValue(ref ClientUpdateTimeStamp);
		serializer.SerializeValue(ref Power);
		serializer.SerializeValue(ref ShootCd);
		serializer.SerializeValue(ref UltPower);
		serializer.SerializeValue(ref Health);
		serializer.SerializeValue(ref bulletCount);
	}
}
public interface IGetLoadOut {
	public PlayerLoadout GetLoadOut();
}

public interface IGetTransform {
	public Transform GetTransform();
}

public interface IGetIInput {
	public IInput GetInput();
}

public interface IAvaterSync : IGetLoadOut,IGetTransform,IGetIInput,INetEntity{
	public NetworkVariable<AvaterState> GetSyncData();
	public void AvaterDataSyncServerRpc(AvaterState data);
	public bool IsController();
	public void Reload();
}