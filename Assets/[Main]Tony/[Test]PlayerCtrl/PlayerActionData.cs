using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionData
{
    public float TimeStamp;
    public Vector2 Pos;
    public Vector2 TargetVec;
    public Vector2 NowVec;
    public float Towards;
    public float RotVec;

    protected PlayerCtrl Player;
    public PlayerActionData(PlayerCtrl player)
    {
        Player = player;
        TimeStamp = Time.time; //todo change to serverSyncTime
        Pos = Player.transform.position;
    }

    public void ServerDataRefresh(PlayerActionData data) {
        Pos = data.Pos;
        TargetVec = data.TargetVec;
        NowVec = data.NowVec;
        Towards = data.Towards;
        RotVec = data.RotVec;
    }
    public void ClientDataRefresh()
    {
        Pos = (Vector2) Player.transform.position;
        TargetVec = Player.InputCtrl.MoveJoy();
    }
    
    public void LocalUpdate() {
        float missTime = Time.time-TimeStamp;
        TimeStamp = Time.time; //todo change to serverSyncTime

        Vector2 vec = TargetVec - NowVec;
        Vector2 direction = vec.normalized;
        Vector2 newVec = TargetVec;
        float distance = vec.magnitude;
        float moveFriction = Player.AvaterData.MoveFriction;
        if (distance > moveFriction) {
            newVec = NowVec+direction * Mathf.Min(moveFriction, distance);
        }
        NowVec = newVec;
      
        float targetTowards = TargetVec != Vector2.zero ? TargetVec.Angle() : Towards;
        Towards = Mathf.SmoothDampAngle(Towards, targetTowards, ref RotVec, Player.AvaterData.RotSpeed);
        Pos = Pos + NowVec * Player.AvaterData.MoveSpeed * missTime;
    }

}