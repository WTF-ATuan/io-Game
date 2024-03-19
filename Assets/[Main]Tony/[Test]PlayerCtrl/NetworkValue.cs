using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class NetworkValue
{
    public delegate Vector2 GetVec2();
    public delegate Vector3 GetVec3();
    public delegate float GetFloat();
    
    public class SyncVec2 : NetworkVariable<Vector2> {
        private float LastValueChangeTime;
        private Vector2 LastValue;
        private Vector2 NowVec;
        private GetVec2 GetVec2;
    
        public void Init(bool isServer, GetVec2 get) {
            OnValueChanged += (oldValue, newValue) => {
                GetVec2 = get;
                var nowTime = Time.time;
                var timeSpace = nowTime - LastValueChangeTime;
                var dis = LastValue - newValue;
                NowVec = dis * (1f / timeSpace);
                LastValueChangeTime = nowTime;
                LastValue = newValue;
            };
        }
    
        public Vector2 GetNow() {
            var nowPos = GetVec2.Invoke();
            var target = Value+(Time.time-LastValueChangeTime)*NowVec;
            var dis = (nowPos - target).magnitude;
            return dis>1?target:Vector2.Lerp(nowPos, target, 0.1f);
        }
    }
    
    public class SyncVec3 : NetworkVariable<Vector3> {
        private float LastValueChangeTime;
        private Vector3 LastValue;
        private Vector3 NowVec;
        private GetVec3 GetVec3;
        
        public void Init(bool isServer, GetVec3 get) {
            GetVec3 = get;
            OnValueChanged += (oldValue, newValue) => {
                var nowTime = Time.time;
                var timeSpace = nowTime - LastValueChangeTime;
                var dis = LastValue - newValue;
                NowVec = dis * (1f / timeSpace);
                LastValueChangeTime = nowTime;
                LastValue = newValue;
            };
        }
    
        public Vector3 GetNow() {
            var nowPos = GetVec3.Invoke();
            var target = Value+(Time.time-LastValueChangeTime)*NowVec;
            var dis = (nowPos - target).magnitude;
            return dis>1?target:Vector2.Lerp(nowPos, target, 0.1f);
        }
    }

    public class Vec3Smoother {
        private float LastValueChangeTime;
        private Vector3 LastValue;
        private Vector3 NowVec;
        private GetVec3 SyncData;
        private GetVec3 LocalData;
        
        public Vec3Smoother(GetVec3 syncData,GetVec3 localData) {
            SyncData = syncData;
            LocalData = localData;
        }

        public void Update() {
            var newValue = SyncData.Invoke();
            var nowTime = Time.time;
            var timeSpace = nowTime - LastValueChangeTime;
            var dis = LastValue - newValue;
            NowVec = dis * (1f / timeSpace);
            LastValueChangeTime = nowTime;
            LastValue = newValue;
        }
        
        public Vector3 Get() {
            var nowPos = LocalData.Invoke();
            var target = SyncData.Invoke()+(Time.time-LastValueChangeTime)*NowVec;
            var dis = (nowPos - target).magnitude;
            return dis>1?target:Vector3.Lerp(nowPos, target,0.1f);
        }
    }
    
    public class Vec2Smoother {
        private float LastValueChangeTime;
        private Vector2 LastValue;
        private Vector2 NowVec;
        private Vector2 SmoothVec;
        private GetVec2 SyncData;
        private GetVec2 LocalData;
        
        public Vec2Smoother(GetVec2 syncData,GetVec2 localData) {
            SyncData = syncData;
            LocalData = localData;
        }

        public void Update() {
            /*
            var newValue = SyncData.Invoke();
            var nowTime = Time.time;
            var timeSpace = nowTime - LastValueChangeTime;
            var dis = LastValue - newValue;
            NowVec = dis * (1f / timeSpace);
            LastValueChangeTime = nowTime;
            LastValue = newValue;
            */
        }
        
        public Vector2 Get() {
            var nowPos = LocalData.Invoke();
            var target = SyncData.Invoke();//+(Time.time-LastValueChangeTime)*NowVec;
            return Vector2.SmoothDamp(nowPos, target, ref SmoothVec, 0.03f);
        }
    }
    
    public class RotSmoother {
        private float LastValueChangeTime;
        private float LastValue;
        private float NowVec;
        private GetFloat SyncData;
        private GetFloat LocalData;
        
        public RotSmoother(GetFloat syncData,GetFloat localData) {
            SyncData = syncData;
            LocalData = localData;
        }

        public void Update() {
            /*
            var newValue = SyncData.Invoke();
            var nowTime = Time.time;
            var timeSpace = nowTime - LastValueChangeTime;
            var dis = LastValue - newValue;
            NowVec = dis * (1f / timeSpace);
            LastValueChangeTime = nowTime;
            LastValue = newValue;
            */
        }
        
        public float Get() {
            var nowPos = LocalData.Invoke();
            var target = SyncData.Invoke();
            return Mathf.SmoothDampAngle(nowPos, target, ref NowVec, 0.03f);
        }
    }
}
