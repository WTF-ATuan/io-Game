using System.Collections;
using System.Collections.Generic;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using Zenject;

public class AvaterData {
    
    public float MoveSpeed;
    public float RotSpeed;
    public float MoveFriction;

    public AvaterData(float moveSpeed,float rotSpeed, float moveFriction) {
        MoveSpeed = moveSpeed;
        RotSpeed = rotSpeed;
        MoveFriction = moveFriction;
    }
}

public interface IAvaterDataCtrl {
    public AvaterData GetData(int ID);
}

public class AvaterDataCtrl : IAvaterDataCtrl {
    
    protected Dictionary<int, AvaterData> Dic;

    public virtual Dictionary<int, AvaterData> DataLoad() {
        var data = new Dictionary<int, AvaterData>();
        data.Add(0,new AvaterData(7f, 0.1f,0.07f));
        return data;
    }

    public AvaterDataCtrl() {
        Dic = DataLoad();
    }
    
    public AvaterData GetData(int id) {
        if (Dic.ContainsKey(id))
            return Dic[id];
        return null;
    }
}

public class ServerAvaterDataCtrl : AvaterDataCtrl {
    public override Dictionary<int, AvaterData> DataLoad() {
        return base.DataLoad();
    }
}
public class DemoAvaterDataCtrl : AvaterDataCtrl { }

public class AvaterDataSystem : MonoInstaller 
{
    public override void InstallBindings() {
        //todo 
        //Container.Bind<IAvaterDataCtrl>().To<ServerAvaterDataCtrl>().FromNew().AsSingle();
        Container.Bind<IAvaterDataCtrl>().To<DemoAvaterDataCtrl>().FromNew().AsSingle();
    }
}
