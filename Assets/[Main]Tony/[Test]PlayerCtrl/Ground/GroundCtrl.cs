using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GroundCtrl : MonoBehaviour, IAStarGround
{
    protected IBattleCtrl BattleCtrl;
    
    [Inject]
    private void Initialization(
        IBattleCtrl battleCtrl
    ) {
        BattleCtrl = battleCtrl;
    }
    
    protected virtual void OnEnable()
    {
        BattleCtrl.AddPad(this);
    }

    protected virtual void OnDisable()
    {
        BattleCtrl.RemovePad(this);
    }
    
    public Vector2Int GetPos() {
        return transform.position.ToVec2Int();
    }

    public virtual bool CanCross()
    {
        return true;
    }
    
}