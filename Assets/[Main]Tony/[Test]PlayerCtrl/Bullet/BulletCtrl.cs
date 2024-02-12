using System;
using System.Collections;
using System.Collections.Generic;
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
    private NetworkVariable<BulletState> SyncData= new NetworkVariable<BulletState>(new BulletState(),
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public void ServerRequest(Vector2 genPos, float angle, float moveSec, float maxDis){
        SetupServerRpc(genPos, angle, moveSec, maxDis);
        Debug.Log($"Server Request");
    }
    

    [ServerRpc]
    private void SetupServerRpc(Vector2 genPos, float angle, float moveSec, float maxDis) {
        Debug.Log($"Server Running!\n" +
                  $"{genPos} \n" +
                  $"{angle}\n" +
                  $"{moveSec}z" );
        SyncData = new NetworkVariable<BulletState>(new BulletState {Pos = genPos},
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        transform.DOMove((genPos + angle.ToVec2() * maxDis), moveSec).SetEase(Ease.Linear).OnComplete(() => {
            gameObject.GetComponent<NetworkObject>().Despawn();
        });
    }

    public void Update() {
        if (IsServer) {
            Debug.Log("Server:"+SyncData.Value.Pos);
            SyncData.Value.Pos = transform.position;
        } else {
            Debug.Log("Client:"+SyncData.Value.Pos);
            transform.position = SyncData.Value.Pos;
        }
    }
}
