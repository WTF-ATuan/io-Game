using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public class CreatureCtrl : NetworkBehaviour {
    protected IBattleCtrl BattleCtrl;

    [Inject]
    private void Initialization(IBattleCtrl battleCtrl)
    {
        BattleCtrl = battleCtrl;
        BattleCtrl.AddCreature(this);
        print("A");
    }
}
