using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HealthBarCtrl : MonoBehaviour
{

    public Transform PowerLine;
    private AvaterAttribute Attribute;
    private AvaterStateData StateData;
    
    public void Setup(AvaterAttribute attribute, AvaterStateData stateData) {
        Attribute = attribute;
        StateData = stateData;
      
    }

    private void Update() {
        PowerLine.localScale = new Vector3(StateData.Power, 1, 1);
    }
}
