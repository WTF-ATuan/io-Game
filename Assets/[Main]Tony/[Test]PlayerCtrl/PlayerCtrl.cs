using System;
using System.Collections;
using System.Collections.Generic;
using Mono.CSharp;
using UnityEngine;
using Zenject;

public class PlayerCtrl : MonoBehaviour {

    public IInput InputCtrl { get; private set; }
    public AvaterData AvaterData { get; private set; }
    
    public PlayerActionData LastActionData{ get; private set; }

    [Inject]
    void Constructor(IAvaterDataCtrl AvaterDataCtrl, IInput inputCtrl)
    {
        InputCtrl = inputCtrl;
        AvaterData = AvaterDataCtrl.GetData(0);
    }
    
    private void FixedUpdate()
    {
        LastActionData = new PlayerActionData(this);
        UpdateAction(LastActionData);
    }

    void UpdateAction(PlayerActionData ActionData) {
        transform.position = ActionData.Pos;
        transform.eulerAngles = new Vector3(0, 0, ActionData.Towards);
    }
}
