using System;
using System.Collections;
using System.Collections.Generic;
using Mono.CSharp;
using Unity.Netcode;
using UnityEngine;



public interface IGetLoadOut {
    public PlayerLoadout GetLoadOut();
}

public interface IGetTransform {
    public Transform GetTransform();
}

public interface IGetIInput {
    public IInput GetInput();
}

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
    public float Health = 1000; //Todo at init we can set Health to maxHealth

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
    }
}

public interface IAvaterSync : IGetLoadOut,IGetTransform,IGetIInput{
    public NetworkVariable<AvaterState> GetSyncData();
    public void AvaterDataSyncServerRpc(AvaterState data);
    public bool IsOwner();
}

public class AvaterStateCtrl  {
    
    private IAvaterSync Avater;
    public AvaterState Data{ get;private set; }

    private Transform RotCenter;

    public AvaterStateCtrl(IAvaterSync avater) {
        Avater = avater;
        Data = new AvaterState();
        RotCenter = Avater.GetTransform().Find("RotCenter");
    }
    
    public void DataSync() {
        if (Avater.IsOwner()) {
            Avater.AvaterDataSyncServerRpc(Data);
        }
    }

    public void ClientUpdate() {
        if (Avater.IsOwner()){
            //--Input
            Data.TargetVec = Avater.GetInput().MoveJoy();
            Data.LastAimPos = Data.AimPos;
            Data.AimPos = Avater.GetInput().AimJoy();
            Data.LastUtlPos = Data.UtlPos;
            Data.UtlPos = Avater.GetInput().UtlJoy();
            //--Input
            
            float missTime = Time.time - Data.ClientUpdateTimeStamp;
            Data.ClientUpdateTimeStamp = Time.time; //todo change to serverSyncTime

            //--Move
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
            //--Move
            
            var weapon = Avater.GetLoadOut().GetWeaponInfo();
            var ultSkill = Avater.GetLoadOut().GetUtlInfo();
            //--Rot
            float targetTowards =  
                Data.IsAim && weapon.TryShoot(Data,false) ? Data.AimPos.Angle() : 
                Data.IsUtl && ultSkill.TryShoot(Data,false) ? Data.UtlPos.Angle() : 
                Data.TargetVec != Vector2.zero ? Data.TargetVec.Angle() : 
                Data.Towards;
            Data.Towards = Mathf.SmoothDampAngle(Data.Towards, targetTowards, ref Data.RotVec, AvaterAttribute.RotSpeed, Mathf.Infinity, missTime);
            //--Rot
            
            //--Shoot
            Data.Power = Mathf.Clamp01(Data.Power + missTime / Avater.GetLoadOut().NowAttribute.PowerChargeToFullSec);
            if (weapon != null && weapon.TryShoot(Data)) {
                Data.Towards = Data.LastAimPos.Angle();
                Data.RotVec = 0;
            } 
            //--Shoot
            
            //--Ult
            Data.UltPower = Mathf.Clamp01(Data.UltPower + missTime / AvaterAttribute.UltPowerChargeToFullSec);
            if (ultSkill != null && ultSkill.TryShoot(Data)) {
                Data.Towards = Data.LastUtlPos.Angle();
                Data.RotVec = 0;
            } 
            //--Ult
        } else {
            Data = Avater.GetSyncData().Value;
        }

        Avater.GetTransform().position = Data.Pos;
        RotCenter.eulerAngles = Vector3.forward*Data.Towards;
    }

    public void ModifyHealth(float amount){
        Data.Health = Mathf.Clamp(Data.Health + amount, 0, Avater.GetLoadOut().NowAttribute.MaxHealth);
        DataSync();
    }
}