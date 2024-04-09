using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


public class HealthBarCtrl : MonoBehaviour{
	public Transform PowerLine;
	public Transform HelthLine;
	public GameObject PowerRoot;
	public Transform PowerPice;
	
	private IGetPlayerLoadout Loadout;
	private AvaterStateCtrl StateData;
	private List<IDisposable> DList;

	protected IBattleCtrl BattleCtrl;
	[Inject]
	private void Initialization(IBattleCtrl battleCtrl) {
		BattleCtrl = battleCtrl;
		DList = new List<IDisposable>();
	}

	public void Setup(IGetPlayerLoadout loadout, AvaterStateCtrl stateData){
		void OnAttributeChange(AvaterAttribute attribute) {
			PowerPice = PowerPice.parent.GetChild(0);
			for (int i = 1; i < PowerPice.parent.childCount; i++) {
				Destroy(PowerPice.parent.GetChild(i));
			}
		
			for (int i = 1; i < attribute.MaxBullet; i++) {
				Instantiate(PowerPice, PowerPice.transform.parent);
			}
		}
		
		Loadout = loadout;
		StateData = stateData;
		transform.SetParent(stateData.RotCenter.parent);
		transform.localPosition = Vector3.zero;
		OnAttributeChange(loadout.GetNowAttribute());
		
		DList.ForEach(e=>e.Dispose());
		DList.Add(EventAggregator.OnEvent<OnAttributeChange>().Subscribe((e) => {
			if (e.Entity.GetEntityID() != StateData.GetEntityID()) return;
			OnAttributeChange(e.Attribute);
		}));
		
		PowerRoot.SetActive(StateData.GetEntityID()==BattleCtrl.GetLocalPlayerID());
	}
	
	private void Update(){
		PowerLine.localScale = new Vector3(1-(StateData.Data.Power), 1, 1);
		HelthLine.localScale = new Vector3(1-(StateData.Data.Health / Loadout.GetNowAttribute().MaxHealth), 1, 1);
		

	}
	
	
}