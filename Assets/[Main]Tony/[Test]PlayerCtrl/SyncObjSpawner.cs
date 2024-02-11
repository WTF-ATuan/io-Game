using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public class SyncObjSpawner : NetworkBehaviour
{

    [Inject]
    private void Initialization(IBattleCtrl battleCtrl) {
        battleCtrl.SetSpawner(this);
    }
    
    public GameObject ButtetPrefab;

    [ServerRpc(RequireOwnership = false)]
    public void SpawnBulletServerRpc(Vector2 genPos, float angle, float moveSec, float maxDis)
    {
        var bullet = Instantiate(ButtetPrefab, genPos, Quaternion.Euler(0,0,angle)).GetComponent<NetworkObject>();
        bullet.Spawn();
        bullet.GetComponent<BulletCtrl>().Setup(genPos, angle, moveSec, maxDis, () => {});//bullet.Despawn(); 
    }
}
