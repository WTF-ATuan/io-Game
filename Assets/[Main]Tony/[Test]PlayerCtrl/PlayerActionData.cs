using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionData
{
    public float TimeStamp;
    public Vector2 Pos;
    public Vector2 TargetVec;
    public Vector2 NowVec;

    public PlayerActionData(PlayerCtrl player)
    {
        TimeStamp = Time.time; //todo change to serverSyncTime
        TargetVec = player.InputCtrl.MoveJoy();
        NowVec = Vector2.zero;

        float missTime = 0;
        if (player.LastActionData != null) {
            NowVec = player.LastActionData.NowVec;
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
        
        Pos = (Vector2)player.transform.position + NowVec * player.AvaterData.MoveSpeed * missTime;
        
    }
}
