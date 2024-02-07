using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvaterStateData
{
    public float TimeStamp;
    
    public Vector2 Pos;
    public Vector2 TargetVec;
    public Vector2 NowVec;
    public Vector2 AimPos;
    public float Towards;
    public float RotVec;

    public float Power;//0~1
    public float ShootCD;

    protected PlayerCtrl Player;
    public AvaterStateData(PlayerCtrl player)
    {
        Player = player;
        TimeStamp = Time.time; //todo change to serverSyncTime
        Pos = Player.transform.position;

        Power = 1;
    }

    public void ServerDataRefresh(AvaterStateData data) {
        Pos = data.Pos;
        TargetVec = data.TargetVec;
        NowVec = data.NowVec;
        Towards = data.Towards;
        RotVec = data.RotVec;
    }
    
    public void ClientDataRefresh()
    {
        float nowTime = Time.time;
        Pos = (Vector2) Player.transform.position;
        TargetVec = Player.InputCtrl.MoveJoy();
        var newAimPos = Player.InputCtrl.AimJoy();

        if (AimPos != Vector2.zero && newAimPos == Vector2.zero) {
            if (ShootCD>nowTime) return;
            int maxBullet = Player.Loadout.NowAttribute.MaxBullet;
            if (maxBullet > 0) {
                float powerNeed = 1f / maxBullet;
                if (Power >= powerNeed) {
                    //todo Shoot
                    Power = Mathf.Clamp01(Power-powerNeed);
                    ShootCD = nowTime+Player.Loadout.NowAttribute.ShootCD;
                    Towards = AimPos.Angle();
                    RotVec = 0;
                }
            }
        }
        AimPos = newAimPos;
    }
    
    public void LocalUpdate() {
        float missTime = Time.time-TimeStamp;
        TimeStamp = Time.time; //todo change to serverSyncTime

        Vector2 vec = TargetVec - NowVec;
        Vector2 direction = vec.normalized;
        Vector2 newVec = TargetVec;
        float distance = vec.magnitude;
        float moveFriction = AvaterAttribute.MoveFriction;
        if (distance > moveFriction) {
            newVec = NowVec+direction * Mathf.Min(moveFriction, distance);
        }
        NowVec = newVec;
        Pos = Pos + NowVec * Player.BaseAttribute.MoveSpeed * missTime;
        
        //Towards
        float targetTowards = AimPos == Vector2.zero ? TargetVec != Vector2.zero ? TargetVec.Angle() : Towards : AimPos.Angle();
        Towards = Mathf.SmoothDampAngle(Towards, targetTowards, ref RotVec, AvaterAttribute.RotSpeed);
        
        //Power
        Power = Mathf.Clamp01(Power+missTime / Player.Loadout.NowAttribute.PowerChargeToFullSec);
    }

}