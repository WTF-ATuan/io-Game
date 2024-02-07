using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using Zenject;

public abstract class Item: ICloneable {
    public abstract object Clone();
}

public class Passive : Item {
    public override object Clone() {
        var clone = new Passive();
        //todo copy some thing
        return clone;
    }
}

public class Rune : Item {
    public override object Clone() {
        var clone = new Passive();
        //todo copy some thing
        return clone;
    }
}

public abstract class InsertThing:Item {
    public int MaxInsert;
    
}

public class UltSkill : InsertThing
{
    public override object Clone() {
        var clone = new UltSkill();
        //todo copy some thing
        return clone;
    }
}

public class Weapon : InsertThing
{
    public Dictionary<AttributeType, float> AttributeBonus;

    public RangePreviewData RangePreview;

    public Weapon(int maxBullet, float powerChargeToFullSec, float damage, float shootCD, RangePreviewData rangePreview) {
        AttributeBonus = new Dictionary<AttributeType, float>();
        AttributeBonus.Add(AttributeType.MaxBullet, maxBullet);
        AttributeBonus.Add(AttributeType.PowerChargeToFullSec, powerChargeToFullSec);
        AttributeBonus.Add(AttributeType.Damage, damage);
        AttributeBonus.Add(AttributeType.ShootCD, shootCD);
        RangePreview = rangePreview;
    }
    
    public override object Clone() {
        var clone = new Weapon(
            (int)AttributeBonus[AttributeType.MaxBullet], 
            AttributeBonus[AttributeType.PowerChargeToFullSec],
            AttributeBonus[AttributeType.Damage],
            AttributeBonus[AttributeType.ShootCD], 
            RangePreview);
        return clone;
    }
}

public class Armor : InsertThing
{
    public Dictionary<AttributeType, int> AttributeBonus;
    public override object Clone() {
        var clone = new Armor();
        clone.AttributeBonus = new Dictionary<AttributeType, int>(this.AttributeBonus);
        return clone;
    }
}
public class PlayerLoadout
{
    protected Weapon Weapon;
    protected Armor Armor;
    protected UltSkill UltSkill;
    
    protected Rune[] WeaponRunes;
    protected Passive[] ArmorPassives;
    protected Rune[] UltSkillRunes;

    private AvaterAttribute BaseAttribute;
    public AvaterAttribute NowAttribute { private set; get; }

    public PlayerLoadout(AvaterAttribute baseAttribute) {
        BaseAttribute = baseAttribute;
        NowAttribute = new AvaterAttribute(BaseAttribute);
    }
    
    public Weapon GetWeaponInfo(out Item[] inserts) {
        return GetInfo(Weapon,WeaponRunes,out inserts);
    }
    
    public Armor GetArmorInfo(out Item[] inserts) {
        return GetInfo(Armor,ArmorPassives,out inserts);
    }
    
    public UltSkill GetUltSkillInfo(out Item[] inserts) {
        return GetInfo(UltSkill,UltSkillRunes,out inserts);
    }

    public bool SetWeapon(Weapon weapon,out List<Item> unload) {
        if (SetThing(Weapon,WeaponRunes, weapon, out unload)) {
            Weapon = weapon;
            WeaponRunes = new Rune[Weapon.MaxInsert];
            NowAttributeUpdate();
            return true;
        }
        return false;
    }
    
    public bool SetArmor(Armor armor,out List<Item> unload) {
        if (SetThing(Armor,ArmorPassives, armor, out unload)) {
            Armor = armor;
            ArmorPassives = new Passive[Armor.MaxInsert];
            NowAttributeUpdate();
            return true;
        }
        return false;
    }
    
    public bool SetUltSkill(UltSkill skill,out List<Item> unload) {
        if (SetThing(UltSkill, UltSkillRunes, skill, out unload)) {
            UltSkill = skill;
            UltSkillRunes = new Rune[UltSkill.MaxInsert];
            return true;
        }
        return false;
    }
    
    public bool AddWeaponRune(Rune rune,int index,out Rune unload) {
        return ArrayInsert(WeaponRunes, rune, index, out unload);
    }
    
    public bool AddArmorPassive(Passive passive,int index,out Passive unload) {
        return ArrayInsert(ArmorPassives, passive, index, out unload);
    }
    
    public bool AddUltSkillRune(Rune rune,int index,out Rune unload) {
        return ArrayInsert(UltSkillRunes, rune, index, out unload);
    }

    private T GetInfo<T>(T thing,Item[] array, out Item[] inserts)where T : InsertThing
    {
        inserts = array.ToArray();
        return (T)thing.Clone();
    }
    
    private bool SetThing<T>(InsertThing oldThing,T[] array,InsertThing newThing,out List<Item> unload) where T : Item {
        unload = new List<Item>();
        if (newThing == null) return false;
        if (oldThing != null) {
            unload.Add(oldThing);
            foreach (var rune in array) {
                if(rune!=null)unload.Add(rune);
            }
        }
        return true;
    }
    
    private bool ArrayInsert<T>(T[] array, T insert,int index,out T unload) where T : class {
        unload = null;
        if (array == null) return false;
        if(index<0||index>array.Length) return false;
        if (array[index] != null) unload = array[index];
        array[index] = insert;
        return true;
    }

    private void NowAttributeUpdate() {
        var data = new AvaterAttribute(BaseAttribute);
        if (Weapon != null) {
            foreach (var attribute in Weapon.AttributeBonus) {
                data.AddAttribute(attribute.Key, attribute.Value);
            }
        }

        if (Armor != null) {
            foreach (var attribute in Armor.AttributeBonus) {
                data.AddAttribute(attribute.Key, attribute.Value);
            }
        }
        NowAttribute.Copy(data);
    }
}

public enum AttributeType {
    MoveSpeed,
    MaxHealth,
    
    MaxBullet,
    PowerChargeToFullSec,
    Damage,
    ShootCD,
}

public class AvaterAttribute {
    
    public float MoveSpeed = 7f;
    public float MaxHealth = 1000;
    
    //Define by Weapon
    public int MaxBullet = 0;
    public float PowerChargeToFullSec = 0;
    public float Damage = 0;
    public float ShootCD = 0;
    
    //NotChange
    public const float HealthChargeToFullSec = 3f;
    public const float RotSpeed = 0.1f;
    public const float MoveFriction = 0.07f;
    public const float CureStartCD = 2;
    
    public AvaterAttribute(float moveSpeed, float maxHealth) {
        MaxHealth = maxHealth;
        MoveSpeed = moveSpeed;
    }
    
    public AvaterAttribute(AvaterAttribute copy) {
        MoveSpeed = copy.MoveSpeed;
        MaxHealth = copy.MaxHealth;
        MaxBullet = copy.MaxBullet;
        PowerChargeToFullSec = copy.PowerChargeToFullSec;
        Damage = copy.Damage;
        ShootCD = copy.ShootCD;
    }
    
    public void Copy(AvaterAttribute copy) {
        MoveSpeed = copy.MoveSpeed;
        MaxHealth = copy.MaxHealth;
        MaxBullet = copy.MaxBullet;
        PowerChargeToFullSec = copy.PowerChargeToFullSec;
        Damage = copy.Damage;
        ShootCD = copy.ShootCD;
    }

    public void AddAttribute(AttributeType type, float value) {
        switch (type)
        {
            case AttributeType.MoveSpeed:
                MoveSpeed += value;
                break;
            case AttributeType.MaxHealth:
                MaxHealth += value;
                break;
            case AttributeType.MaxBullet:
                MaxBullet += (int)value;
                break;
            case AttributeType.PowerChargeToFullSec:
                PowerChargeToFullSec += value;
                break;
            case AttributeType.Damage:
                Damage += value;
                break;
            case AttributeType.ShootCD:
                ShootCD += value;
                break;
        }
    }
}

public interface IAvaterAttributeCtrl {
    public AvaterAttribute GetData(int ID = 0);
}

public class AvaterAttributeCtrl : IAvaterAttributeCtrl {
    
    protected Dictionary<int, AvaterAttribute> Dic;

    public virtual Dictionary<int, AvaterAttribute> DataLoad() {
        var data = new Dictionary<int, AvaterAttribute>();
        data.Add(0,new AvaterAttribute(7, 1000));
        return data;
    }

    public AvaterAttributeCtrl() {
        Dic = DataLoad();
    }
    
    public AvaterAttribute GetData(int id) {
        if (Dic.ContainsKey(id))
            return Dic[id];
        return null;
    }
}

public class ServerAvaterAttributeCtrl : AvaterAttributeCtrl {
    public override Dictionary<int, AvaterAttribute> DataLoad() {
        return base.DataLoad();
    }
}
public class DemoAvaterAttributeCtrl : AvaterAttributeCtrl { }

public class AvaterDataSystem : MonoInstaller 
{
    public override void InstallBindings() {
        //todo 
        //Container.Bind<IAvaterDataCtrl>().To<ServerAvaterDataCtrl>().FromNew().AsSingle();
        Container.Bind<IAvaterAttributeCtrl>().To<DemoAvaterAttributeCtrl>().FromNew().AsSingle().NonLazy();
    }
}
