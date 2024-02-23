using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace _Main_Tony._Test_PlayerCtrl.Runes{
	[CreateAssetMenu(fileName = "RunesMapper", menuName = "Main/RunesDataInstaller", order = 0)]
	public class RunesDataInstaller : ScriptableObjectInstaller{
		public RunesMapper runesMapper;
		public override void InstallBindings(){
			Container.Bind<RunesMapper>().FromInstance(runesMapper).AsSingle().NonLazy();
		}
	}

	[Serializable]
	public class RunesMapper{
		[SerializeField] private List<RunesData> runesData;
		public GameObject GetViewObjectById(string id){
			return runesData.Find(x => x.id.Equals(id)).viewPrefab;
		}
	}

	[Serializable]
	public class RunesData{
		public string id;
		public GameObject viewPrefab;
	}
}