using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.CSharp;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public class PlayerCtrl : MonoBehaviour {

    public IInput InputCtrl { get; private set; }
    public AvaterData AvaterData { get; private set; }
    
    public PlayerActionData ActionData{ get; private set; }

    [Inject]
    void Constructor(IAvaterDataCtrl avaterDataCtrl, IInput inputCtrl,IBattleCtrl battleCtrl)
    {
        InputCtrl = inputCtrl;
        AvaterData = avaterDataCtrl.GetData(0);
        ActionData = new PlayerActionData(this);
        battleCtrl.SetLocalPlayer(this);
    }

    private void Start()
    {
        ServerTest();
    }

    async Task ServerTest() {
        while (true) {
            ActionData.ClientDataRefresh();//todo ServerDataRefresh
            await Task.Delay(50);
        }
    }

    private void FixedUpdate()
    {
        ActionData.LocalUpdate();
        UpdateAction();
    }

    void UpdateAction() {
        transform.position = ActionData.Pos;
        transform.eulerAngles = new Vector3(0,0,ActionData.Towards);
    }
    
}
