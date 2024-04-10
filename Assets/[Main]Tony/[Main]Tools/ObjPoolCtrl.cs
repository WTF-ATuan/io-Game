using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate GameObject GetGameObject();

public class ObjPoolCtrl<T>:IDisposable where T : Component {
    
    private GetGameObject CreateFunc;
    private Action DisposFunc;
    private List<GameObject> ActivePool;
    private List<GameObject> UnActivePool;
    private List<PoolObj<T>> ActivePoolObjs;
    private List<int> EveageActiveCount;
    private int ActivePoolSize;
    private int MinPoolSize;
    private int PoolSize => ActivePoolSize < MinPoolSize ? MinPoolSize : ActivePoolSize;
    private Transform Pool;
    public bool IsLoaded { private set; get; }

    public ObjPoolCtrl(string path, Transform parent, int minSize = 1)
    {
        var loader = AdbLoader.Load<GameObject>(path, obj => {
            CreateFunc = () => GameObject.Instantiate(obj, parent);
            IsLoaded = true;
        });

        DisposFunc = () => {
            loader.Dispose();
        };
        
        MinPoolSize = minSize;
        ActivePool = new List<GameObject>();
        UnActivePool = new List<GameObject>();
        ActivePoolObjs = new List<PoolObj<T>>();
        EveageActiveCount = new List<int>();

        Pool = new GameObject("[Pool]"+typeof(T).Name).transform;
        Debug.Log("ccc");
    }
    
    public ObjPoolCtrl(GameObject obj, Transform parent, int minSize = 1) : 
        this(() => GameObject.Instantiate(obj,parent), minSize) {
    }
    
    public ObjPoolCtrl(GetGameObject createFunc, int minSize = 1)
    {
        CreateFunc = createFunc;
        MinPoolSize = minSize;
        ActivePool = new List<GameObject>();
        UnActivePool = new List<GameObject>();
        ActivePoolObjs = new List<PoolObj<T>>();
        EveageActiveCount = new List<int>();
        IsLoaded = true;
        
        Pool = new GameObject("[Pool]"+typeof(T).Name).transform;
    }
    
    public List<PoolObj<T>> GetAllActiveObj()
    {
        return new List<PoolObj<T>>(ActivePoolObjs);
    }

    public void Clean()
    {
        GetAllActiveObj().ForEach(e=>e.Dispose());
    }
    
    public PoolObj<T> Get()
    {
        GameObject o;
        if (UnActivePool.Count == 0) {
            o = Create();
            o.SetActive(true);
        } else {
            o = UnActivePool[0];
            UnActivePool.Remove(o);
            ActivePool.Add(o);
            o.transform.SetAsLastSibling();
            o.SetActive(true);
        }

        Cheak();
        var obj = new PoolObj<T>(o, (e) => {
            o.SetActive(false);
            o.transform.parent = Pool;
            
            ActivePool.Remove(o);
            ActivePoolObjs.Remove(e);
            Cheak();
            
            if(UnActivePool.Count<PoolSize)
                UnActivePool.Add(o);
            else 
                GameObject.Destroy(o);
        });
        ActivePoolObjs.Add(obj);
        return obj;
    }

    GameObject Create() {
        var o = CreateFunc();
        ActivePool.Add(o);
        return o;
    }

    void Cheak()
    {
        EveageActiveCount.Add(ActivePool.Count);
        if(EveageActiveCount.Count>10)EveageActiveCount.RemoveAt(0);
        float all = 0;
        EveageActiveCount.ForEach(e=>all+=e);
        all /= EveageActiveCount.Count;
        ActivePoolSize = Mathf.FloorToInt(all) + 1;
    }
    
    public void Dispose() {
        DisposFunc?.Invoke();
    }
    
}

public class PoolObj<T>:IDisposable where T : Component{
    
    public GameObject Obj;
    public T Ctrl;
    private Action<PoolObj<T>> OnDispose;
    
    public PoolObj(GameObject obj,Action<PoolObj<T>> onDispose) {
        Obj = obj;
        Ctrl = Obj.GetComponent<T>();
        OnDispose = onDispose;
    }
    
    public void Dispose() {
        OnDispose.Invoke(this);
    }
}
