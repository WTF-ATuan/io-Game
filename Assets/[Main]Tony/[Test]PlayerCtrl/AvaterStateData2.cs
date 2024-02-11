using System;
using System.Collections;
using System.Collections.Generic;
using Mono.CSharp;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class AvaterSyncData3 : INetworkSerializable
{
    public Vector2 Pos;
    public Vector2 TargetVec;
    public Vector2 NowVec;
    public Vector2 AimPos;
    public Vector2 LastAimPos;
    public float Ang;
    public float ClientUpdateTimeStamp;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Pos);
        serializer.SerializeValue(ref TargetVec);
        serializer.SerializeValue(ref NowVec);
        serializer.SerializeValue(ref AimPos);
        serializer.SerializeValue(ref LastAimPos);
        serializer.SerializeValue(ref Ang);
        serializer.SerializeValue(ref ClientUpdateTimeStamp);
    }

    public void Copy(AvaterSyncData3 copy)
    {
        Pos = copy.Pos;
        TargetVec = copy.TargetVec;
        NowVec = copy.NowVec;
        AimPos = copy.AimPos;
        LastAimPos = copy.LastAimPos;
        Ang = copy.Ang;
        ClientUpdateTimeStamp = copy.ClientUpdateTimeStamp;
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

public interface IAvaterSync : IGetLoadOut,IGetTransform,IGetIInput{
    public NetworkVariable<AvaterSyncData3> GetSyncData();
    public void AvaterDataSyncServerRpc(AvaterSyncData3 data);

    public bool IsOwner();
}

public class AvaterStateData2  {
    
    private IAvaterSync Avater;
    public AvaterSyncData3 Data{ get;private set; }

    public AvaterStateData2(IAvaterSync avater) {
        Avater = avater;
        Data = new AvaterSyncData3();
    }
    
    public void DataSync() {
        if (Avater.IsOwner()) {
            Avater.AvaterDataSyncServerRpc(Data);
        }
    }

    public void ClientUpdate() {
        if (Avater.IsOwner()) {
            Data.TargetVec = Avater.GetInput().MoveJoy();
            
            float missTime = Time.time - Data.ClientUpdateTimeStamp;
            Data.ClientUpdateTimeStamp = Time.time; //todo change to serverSyncTime

            Vector2 vec = Data.TargetVec - Data.NowVec;
            Vector2 direction = vec.normalized;
            Vector2 newVec = Data.TargetVec;
            float distance = vec.magnitude;
            float moveFriction = AvaterAttribute.MoveFriction;
            if(distance > moveFriction){
                newVec = Data.NowVec + direction * Mathf.Min(moveFriction, distance);
            }

            Data.NowVec = newVec;
            Data.Pos = Data.Pos + Data.NowVec * Avater.GetLoadOut().NowAttribute.MoveSpeed * missTime;
        } else {
            Data = Avater.GetSyncData().Value;
        }

        Avater.GetTransform().position = Data.Pos;
    }   
}
