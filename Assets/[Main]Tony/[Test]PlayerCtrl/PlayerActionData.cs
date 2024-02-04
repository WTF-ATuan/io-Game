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

    public PlayerActionData(PlayerCtrl player)
    {
        TimeStamp = Time.time; //todo change to serverSyncTime
        TargetVec = player.InputCtrl.MoveJoy();
        NowVec = Vector2.zero;
        Towards = 0;
        RotVec = 0;
        
        float missTime = 0;
        if (player.LastActionData != null) {
            NowVec = player.LastActionData.NowVec;
            Towards = player.LastActionData.Towards;
            RotVec = player.LastActionData.RotVec;
            missTime = TimeStamp - player.LastActionData.TimeStamp;
        }

        Vector2 vec = TargetVec - NowVec;
        Vector2 direction = vec.normalized;
        Vector2 newVec = TargetVec;
        float distance = vec.magnitude;
        float moveFriction = player.AvaterData.MoveFriction;
        if (distance > moveFriction) {
            newVec = NowVec+direction * Mathf.Min(moveFriction, distance);
        }
        NowVec = newVec;
      
        float targetTowards = TargetVec != Vector2.zero ? TargetVec.Angle() : Towards;
        //Towards = Towards.RotateTowards(targetTowards, player.AvaterData.RotSpeed*missTime);
        Towards = Mathf.SmoothDampAngle(Towards, targetTowards, ref RotVec, player.AvaterData.RotSpeed);
        Pos = (Vector2)player.transform.position + NowVec * player.AvaterData.MoveSpeed * missTime;

 
    }
}
