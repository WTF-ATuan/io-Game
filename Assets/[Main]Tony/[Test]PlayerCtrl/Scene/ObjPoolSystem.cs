using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ObjPoolSystem : MonoInstaller {
    
    public GameObject HealthBarPrefab;
    public Image ultUI;
    
    public override void InstallBindings() {
        Container.Bind<ObjPoolCtrl<HealthBarCtrl>>().FromInstance(new ObjPoolCtrl<HealthBarCtrl>(HealthBarPrefab,null)).AsSingle();
        //Container.Bind<Image>().WithId("ult").FromInstance(ultUI).AsSingle();
    }
    
}