using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RangePreviewCtrl : MonoBehaviour
{
    public MeshRenderer Mesh;
    private Material M;
    private AvaterStateCtrl PlayerData;
    private IGetPlayerLoadout PlayerLoadout;
    
    private static readonly int Width = Shader.PropertyToID("_Width");
    private static readonly int Type = Shader.PropertyToID("_Type");
    private static readonly int Radius = Shader.PropertyToID("_Radius");

    private void Start() {
        M = Mesh.material;
        gameObject.SetActive(false);
    }

    public void Init(AvaterStateCtrl data, IGetPlayerLoadout playerLoadout) {
        PlayerData = data;
        PlayerLoadout = playerLoadout;
    }
    
    public void Update() {
        if (PlayerData == null) return;
        RangePreviewData data = null;
        float ang = 0;
        float radius = 0;
        if (PlayerData.Data.IsAim) {
            var weapon = PlayerLoadout.GetWeaponInfo();
            if (weapon.TryShoot(PlayerData.Data, false)) {
                data = weapon.RangePreview;
                ang = PlayerData.Data.AimPos.Angle();
                radius = PlayerData.Data.AimPos.magnitude;
            }
        }
        if (PlayerData.Data.IsUtl) {
            var utl = PlayerLoadout.GetUtlInfo();
            if (utl.TryShoot(PlayerData.Data, false)) {
                data = utl.RangePreview;
                ang = PlayerData.Data.UtlPos.Angle();
                radius = PlayerData.Data.UtlPos.magnitude;
            }
        }
        if (data == null) {
            if(gameObject.activeSelf)gameObject.SetActive(false);
            return;
        }
        transform.eulerAngles = new Vector3(0, 0, ang);
        M.SetFloat(Width, data.Width/360);
        M.SetFloat(Radius, radius);
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
public class RangePreviewData {
    public RangePreviewType Type;
    public float Dis;
    public float Width;

    public RangePreviewData(RangePreviewType type,float dis,float width) {
        Type = type;
        Dis = dis;
        Width = width;
    }
    
}
