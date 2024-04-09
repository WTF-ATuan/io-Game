using System;
using Random = UnityEngine.Random;

namespace MatchMaking_Prototype.Battle{
	[Serializable]
	public class BattleLevelInfo{
		public BattleType battleType = BattleType.Pvp;

		public void RandomSelect(){
			var typeCount = Enum.GetNames(typeof(BattleType)).Length;
			var randomNum = Random.Range(0, typeCount);
			var selectedType = (BattleType)Enum.ToObject(typeof(BattleType), randomNum);
			battleType = selectedType;
		}
	}

	public enum BattleType{
		Pvp,
		Pve,
	}
}