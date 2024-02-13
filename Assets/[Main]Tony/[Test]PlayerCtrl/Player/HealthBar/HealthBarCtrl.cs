using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HealthBarCtrl : MonoBehaviour{
	public Transform PowerLine;
	public Transform HelthLine;
	private AvaterAttribute Attribute;
	private AvaterStateCtrl StateData;

	public void Setup(AvaterAttribute attribute, AvaterStateCtrl stateData){
		Attribute = attribute;
		StateData = stateData;
	}

	private void Update(){
		PowerLine.localScale = new Vector3(StateData.Data.Power, 1, 1);
		HelthLine.localScale = new Vector3(StateData.Data.Health / Attribute.MaxHealth, 1, 1);
	}
}