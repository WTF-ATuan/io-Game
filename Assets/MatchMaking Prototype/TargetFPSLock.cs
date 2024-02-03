using System;
using UnityEngine;

namespace Assets.Scenes{
	public class TargetFPSLock : MonoBehaviour{
		private void Awake(){
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 60;
		}
	}
}