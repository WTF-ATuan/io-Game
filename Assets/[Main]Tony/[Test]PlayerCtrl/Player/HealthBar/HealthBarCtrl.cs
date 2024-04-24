using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


public class HealthBarCtrl : MonoBehaviour{
	public Transform HelthLine;
	public Image bulletRoundImage;
	public TMP_Text bulletText;

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
		Loadout = loadout;
		StateData = stateData;
		transform.SetParent(stateData.RotCenter.parent);
		transform.localPosition = Vector3.zero;
		
		DList.ForEach(e=>e.Dispose());
		DList.Add(EventAggregator.OnEvent<OnAttributeChange>().Subscribe((e) => {
			if (e.Entity.GetEntityID() != StateData.GetEntityID()) return;
		}));
	}
	
	private void Update(){	
		bulletRoundImage.fillAmount = (float)StateData.Data.bulletCount / (float)Loadout.GetNowAttribute().BulletMaxCount;
		bulletText.text = StateData.Data.bulletCount.ToString();
		HelthLine.localScale = new Vector3(1-(StateData.Data.Health / Loadout.GetNowAttribute().MaxHealth), 1, 1);
	}
	
	
}