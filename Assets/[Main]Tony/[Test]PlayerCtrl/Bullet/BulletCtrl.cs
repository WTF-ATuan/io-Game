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
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref Pos);
    }
}

public class BulletCtrl : NetworkBehaviour
{
    private Action OnDead;
    private NetworkVariable<BulletState> SyncData= new(new BulletState());
    
    public void Setup(Vector2 genPos, float angle, float moveSec, float maxDis,Action onFinish)
    {
        transform.position = genPos;
        transform.eulerAngles = Vector3.forward*angle;
        transform.DOMove(genPos + angle.ToVec2() * maxDis, moveSec).OnComplete(() => { onFinish.Invoke();});
    }
    
    private void Update() {
        if (IsServer) {
            SyncData.Value = new BulletState{Pos = transform.position};
        } else {
            transform.position = SyncData.Value.Pos;
        }
    }
}
