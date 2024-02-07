using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ObjPoolSystem : MonoInstaller 
{
    public GameObject HealthBarPrefab;
    
    public override void InstallBindings() {
        Container.Bind<ObjPoolCtrl<HealthBarCtrl>>().FromInstance(new ObjPoolCtrl<HealthBarCtrl>(HealthBarPrefab,null)).AsSingle();
    }
    
}