using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class BulletState : INetworkSerializable {
    
    public Vector3 Pos;
    public float TimeStamp;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref Pos);
        serializer.SerializeValue(ref TimeStamp);
    }
}

public class SyncVec3 : NetworkVariable<Vector3> {
    private float LastValueChangeTime;
    private Vector3 LastValue;
    private Vector3 NowVec;
    
    public SyncVec3() {
        OnValueChanged += (oldValue, newValue) => {
            var nowTime = Time.time;
            var timeSpace = nowTime - LastValueChangeTime;
            var dis = LastValue - newValue;
            NowVec = dis * (1f / timeSpace);
            LastValueChangeTime = nowTime;
            LastValue = newValue;
        };
    }
    
    public Vector3 GetNow(Vector3 nowVec) {
        var target = Value+(Time.time-LastValueChangeTime)*NowVec;
        var dis = (nowVec - target).magnitude;
        return dis>1?target:Vector2.Lerp(nowVec, target, 0.1f);
    }
}

public class BulletCtrl : NetworkBehaviour
{
    private Action OnDead;
    private SyncVec3 Data = new();

    public override void OnNetworkSpawn() {}
    
    public void Setup(Vector2 genPos, float angle, float moveSec, float maxDis,Action onFinish)
    {
        transform.position = genPos;
        transform.eulerAngles = Vector3.forward*angle;
        transform.DOMove(genPos + angle.ToVec2() * maxDis, moveSec).OnComplete(() => { onFinish.Invoke();});
    }
    
    private void Update() {
        if (IsServer) {
            Data.Value = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        } else {
            transform.position = Data.GetNow(transform.position);
        }
    }
}
