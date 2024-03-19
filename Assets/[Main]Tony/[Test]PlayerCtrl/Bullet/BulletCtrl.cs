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

public class BulletCtrl : NetworkBehaviour
{
    private Action OnDead;
    private NetworkVariable<Vector3> Data = new();
    private NetworkValue.Vec3Smoother _vec3Smoother;
    private bool IsInit = false;
    
    public override void OnNetworkSpawn()
    {
        _vec3Smoother = new NetworkValue.Vec3Smoother(() => Data.Value, () => transform.position);
        if (IsClient) {
            Data.OnValueChanged+= (value, newValue) => {
                _vec3Smoother.Update();
                if (IsInit) {
                    transform.position = Data.Value;
                }
                IsInit = true;
            };
        }
    }
    
    public void Setup(Vector2 genPos, float angle, float moveSec, float maxDis,Action onFinish) {
        transform.position = genPos;
        transform.eulerAngles = Vector3.forward*angle;
        transform.DOMove(genPos + angle.ToVec2() * maxDis, moveSec).OnComplete(() => { onFinish.Invoke();});
    }
    
    private void Update() {
        if (IsServer) {
            Data.Value = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        } else {
            transform.position = _vec3Smoother.Get();
        }
    }
}
