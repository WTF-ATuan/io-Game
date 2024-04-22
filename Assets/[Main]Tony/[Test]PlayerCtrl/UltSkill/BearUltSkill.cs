using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class BearUltSkill : UltSkill {

    public BearUltSkill() {
        RangePreview = new RangePreviewData(RangePreviewType.Throw, 3, 0.5f);
    }
    
    [Inject]
    private void Initialization(IBattleCtrl battleCtrl) {
        BattleCtrl = battleCtrl;
    }

    protected override void OnShoot(AvaterState data)
    {
        // BattleCtrl.GetSpawner().SpawnMobServerRpc(data.Pos+data.LastUtlPos*RangePreview.Dis, data.Towards);
    }
}
