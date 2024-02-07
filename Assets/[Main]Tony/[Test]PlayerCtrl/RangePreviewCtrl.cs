using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangePreviewCtrl : MonoBehaviour
{
    public MeshRenderer Mesh;
    private Material M;
    
    private static readonly int Towards = Shader.PropertyToID("_Towards");
    private static readonly int Radius = Shader.PropertyToID("_Radius");
    private static readonly int SectorAngle = Shader.PropertyToID("_SectorAngle");

    private void Start() {
        M = Mesh.material;
    }
    
    public void Setup() {
        if(gameObject.activeSelf)gameObject.SetActive(false);
    }

    public void Setup(RangePreviewData data, float towards) {
        M.SetFloat(Towards, towards);
        M.SetFloat(Radius, data.Radius);
        M.SetFloat(SectorAngle, data.SectorAngle);
        if(!gameObject.activeSelf) gameObject.SetActive(true);
    }
}

public struct RangePreviewData {
    public float Radius;
    public float SectorAngle;
    
}
