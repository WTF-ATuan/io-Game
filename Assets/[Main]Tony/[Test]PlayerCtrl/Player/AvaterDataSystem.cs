using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public abstract class Item{ }

public class Passive : Item{ }

public class Rune : Item{ }

public abstract class InsertThing : Item{
	public int MaxInsert;
}

public abstract class UltSkill : InsertThing{
	public RangePreviewData RangePreview;
	protected IBattleCtrl BattleCtrl;

	[Inject]
	private void Initialization(IBattleCtrl battleCtrl){
		BattleCtrl = battleCtrl;
	}

	protected abstract void OnShoot(AvaterState data);

	public virtual bool TryShoot(AvaterState data, bool forceShoot = true){
		if(((data.UtlPos == Vector2.zero && data.LastUtlPos != Vector2.zero) || !forceShoot) &&
		   data.UltPower >= 1){
			if(forceShoot){
				data.UltPower = 0;
				OnShoot(data);
			}

			return true;
		}

		return false;
	}
}


public interface IWeaponFactory{
	T Create<T>(params object[] parameters) where T : Weapon;
}

public class WeaponFactory : IWeaponFactory{
	private readonly DiContainer _container;

	public WeaponFactory(DiContainer container){
		_container = container;
	}

	public T Create<T>(params object[] parameters) where T : Weapon{
		var instance = (T)Activator.CreateInstance(typeof(T), parameters);
		_container.Inject(instance);
		return instance;
	}
}

public interface IUltSkillFactory{
	T Create<T>(params object[] parameters) where T : UltSkill;
}

public class UltSkillFactory : IUltSkillFactory{
	private readonly DiContainer _container;

	public UltSkillFactory(DiContainer container){
		_container = container;
	}

	public T Create<T>(params object[] parameters) where T : UltSkill{
		var instance = (T)Activator.CreateInstance(typeof(T), parameters);
		_container.Inject(instance);
		return instance;
	}
}

public abstract class Weapon : InsertThing{
	protected INetEntity Owner;
	public Dictionary<AttributeType, float> AttributeBonus;
	public RangePreviewData RangePreview;
	public bool IsPauseAim{ protected set; get; }
	public bool IsPauseMove{ protected set; get; }
	public bool IsShooting{ protected set; get; }

	protected IBattleCtrl BattleCtrl;
	protected WeaponData weaponData;

	[Inject]
	private void Initialization(IBattleCtrl battleCtrl){
		BattleCtrl = battleCtrl;
	}

	public void OnEquip(INetEntity owner){
		Owner = owner;
	}

	public abstract Task OnShoot(AvaterState data);

	protected virtual void Shoot(AvaterState data){
		async Task ShootDelay(){
			IsShooting = true;
			await OnShoot(data);
			IsShooting = false;
		}

		ShootDelay();
	}

	public virtual bool TryShoot(AvaterState data, bool forceShoot = true){
		if(IsShooting) return false;
		var nowTime = Time.time;
		if(((data.AimPos == Vector2.zero && data.LastAimPos != Vector2.zero) || !forceShoot) &&
		   data.ShootCd < nowTime){
			if(forceShoot){
				data.ShootCd = nowTime + AttributeBonus[AttributeType.ShootCd];
				Shoot(data);
			}

			return true;
		}

		return false;
	}

	public Weapon(WeaponData weaponData){
		AttributeBonus = new Dictionary<AttributeType, float>{
			{ AttributeType.MaxBullet, weaponData.MaxBullet },
			{ AttributeType.Damage, weaponData.Damage },
			{ AttributeType.ShootCd, weaponData.ShootCd },
			{ AttributeType.FlySec, weaponData.FlySec },
			{ AttributeType.FlyDis, weaponData.FlyDis }
		};
		this.weaponData = weaponData;
		RangePreview = weaponData.RangePreview;
	}

	protected BulletData GetDefaultBullet(AvaterState data){
		var bulletData = new BulletData{
			genPos = data.Pos,
			angle = data.Towards,
			flySec = AttributeBonus[AttributeType.FlySec],
			flyDis = AttributeBonus[AttributeType.FlyDis],
			damage = AttributeBonus[AttributeType.Damage],
			playerId = Owner.GetEntityID(),
			runesId = "Default"
		};
		return bulletData;
	}
}

public class Armor : InsertThing{
	public Dictionary<AttributeType, int> AttributeBonus;
}

public interface IGetPlayerLoadout{
	public Weapon GetWeaponInfo();
	public Armor GetArmorInfo();
	public UltSkill GetUtlInfo();
	public Weapon GetWeaponInfo(out Item[] inserts);
	public Armor GetArmorInfo(out Item[] inserts);
	public UltSkill GetUtlInfo(out Item[] inserts);

	public AvaterAttribute GetNowAttribute();
}

public class OnAttributeChange{
	public AvaterAttribute Attribute;
	public INetEntity Entity;

	public OnAttributeChange(INetEntity entity, AvaterAttribute attribute){
		Entity = entity;
		Attribute = attribute;
	}
}

public class PlayerLoadout : IGetPlayerLoadout{
	protected INetEntity Entity;

	protected Weapon Weapon;
	protected Armor Armor;
	protected UltSkill UltSkill;

	protected Rune[] WeaponRunes;
	protected Passive[] ArmorPassives;
	protected Rune[] UltSkillRunes;

	private AvaterAttribute BaseAttribute;
	public AvaterAttribute NowAttribute{ get; private set; }

	private ISyncAttribute _syncAttribute;

	public PlayerLoadout(AvaterAttribute baseAttribute, INetEntity entity
		, ISyncAttribute syncAttribute){
		Entity = entity;
		BaseAttribute = baseAttribute;
		NowAttribute = new AvaterAttribute(BaseAttribute);
		_syncAttribute = syncAttribute;
	}

	public Weapon GetWeaponInfo(){
		return GetWeaponInfo(out Item[] inserts);
	}

	public Armor GetArmorInfo(){
		return GetArmorInfo(out Item[] inserts);
	}

	public UltSkill GetUtlInfo(){
		return GetUtlInfo(out Item[] inserts);
	}

	public Weapon GetWeaponInfo(out Item[] inserts){
		return GetInfo(Weapon, WeaponRunes, out inserts);
	}

	public Armor GetArmorInfo(out Item[] inserts){
		return GetInfo(Armor, ArmorPassives, out inserts);
	}

	public UltSkill GetUtlInfo(out Item[] inserts){
		return GetInfo(UltSkill, UltSkillRunes, out inserts);
	}

	public AvaterAttribute GetNowAttribute(){
		return _syncAttribute.GetAttributeData().Value;
	}

	public bool SetWeapon(Weapon weapon, out List<Item> unload){
		if(SetThing(Weapon, WeaponRunes, weapon, out unload)){
			Weapon = weapon;
			WeaponRunes = new Rune[Weapon.MaxInsert];
			Weapon.OnEquip(Entity);
			NowAttributeUpdate();
			return true;
		}

		return false;
	}

	private T GetInfo<T>(T thing, Item[] array, out Item[] inserts) where T : InsertThing{
		inserts = array == null ? Array.Empty<Item>() : array.ToArray();
		return thing; //(T)thing.Clone();
	}

	private bool SetThing<T>(InsertThing oldThing, T[] array, InsertThing newThing, out List<Item> unload)
			where T : Item{
		unload = new List<Item>();
		if(newThing == null) return false;
		if(oldThing != null){
			unload.Add(oldThing);
			foreach(var rune in array){
				if(rune != null) unload.Add(rune);
			}
		}

		return true;
	}

	private void NowAttributeUpdate(){
		var data = new AvaterAttribute(BaseAttribute);
		if(Weapon != null){
			data.damage = Weapon.AttributeBonus[AttributeType.Damage];
			data.maxBullet = (int)Weapon.AttributeBonus[AttributeType.MaxBullet];
			data.shootCd = Weapon.AttributeBonus[AttributeType.ShootCd];
		}

		if(_syncAttribute.IsController()){
			NowAttribute.Copy(data);
			_syncAttribute.AttributeDataSyncServerRpc(NowAttribute);
		}
		else{
			NowAttribute = _syncAttribute.GetAttributeData().Value ?? data;
		}

		EventAggregator.Publish(new OnAttributeChange(Entity, NowAttribute));
	}
}

public enum AttributeType{
	MoveSpeed,
	MaxHealth,
	MaxBullet,
	Damage,
	ShootCd,
	FlySec,
	FlyDis
}

public interface IAvaterAttributeCtrl{
	public AvaterAttribute GetData(int ID = 0);
}

public class AvaterAttributeCtrl : IAvaterAttributeCtrl{
	protected Dictionary<int, AvaterAttribute> Dic;

	public virtual Dictionary<int, AvaterAttribute> DataLoad(){
		var data = new Dictionary<int, AvaterAttribute>();
		data.Add(0, new AvaterAttribute());
		data.Add(1, new AvaterAttribute());
		return data;
	}

	public AvaterAttributeCtrl(){
		Dic = DataLoad();
	}

	public AvaterAttribute GetData(int id){
		if(Dic.ContainsKey(id))
			return Dic[id];
		return null;
	}
}

public class DemoAvaterAttributeCtrl : AvaterAttributeCtrl{ }

public class AvaterDataSystem : MonoInstaller{
	public override void InstallBindings(){
		Container.Bind<IAvaterAttributeCtrl>().To<DemoAvaterAttributeCtrl>().FromNew().AsSingle().NonLazy();
		Container.Bind<IWeaponFactory>().To<WeaponFactory>().AsSingle().WithArguments(Container);
		Container.Bind<IUltSkillFactory>().To<UltSkillFactory>().AsSingle().WithArguments(Container);
	}
}