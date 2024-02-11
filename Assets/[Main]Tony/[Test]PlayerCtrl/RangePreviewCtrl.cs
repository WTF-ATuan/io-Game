using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangePreviewCtrl : MonoBehaviour
{
    public MeshRenderer Mesh;
    private Material M;
    
    private static readonly int Width = Shader.PropertyToID("_Width");
    private static readonly int Type = Shader.PropertyToID("_Type");

    private void Start() {
        M = Mesh.material;
    }
    
    public void Setup() {
        if(gameObject.activeSelf)gameObject.SetActive(false);
    }

    public void Setup(RangePreviewData data, float towards) {
        transform.eulerAngles = new Vector3(0, 0, towards);
        M.SetFloat(Width, data.Width/360);
        M.SetFloat(Type, (int)data.Type);
        transform.localScale = Vector3.one*data.Dis*0.2f;
        if(!gameObject.activeSelf) gameObject.SetActive(true);
    }
}

public enum RangePreviewType {
    Sector,
    Straight,
    Throw
}
public struct RangePreviewData {
    public RangePreviewType Type;
    public float Dis;
    public float Width;
    
}
