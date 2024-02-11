using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HealthBarCtrl : MonoBehaviour
{

    public Transform PowerLine;
    private AvaterAttribute Attribute;
    private AvaterStateCtrl StateData;
    
    public void Setup(AvaterAttribute attribute, AvaterStateCtrl stateData) {
        Attribute = attribute;
        StateData = stateData;
      
    }

    private void Update() {
        PowerLine.localScale = new Vector3(StateData.Data.Power, 1, 1);
    }
}
