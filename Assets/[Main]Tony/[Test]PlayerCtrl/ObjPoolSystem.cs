using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ObjPoolSystem : MonoInstaller {
    
    public GameObject HealthBarPrefab;
    public GameObject ButtetPrefab;
    
    public override void InstallBindings() {
        Container.Bind<ObjPoolCtrl<HealthBarCtrl>>().FromInstance(new ObjPoolCtrl<HealthBarCtrl>(HealthBarPrefab,null)).AsSingle();
        Container.Bind<ObjPoolCtrl<BulletCtrl>>().FromInstance(new ObjPoolCtrl<BulletCtrl>(ButtetPrefab,null)).AsSingle();
    }
    
}