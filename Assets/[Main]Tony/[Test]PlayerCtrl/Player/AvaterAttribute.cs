using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class AvaterAttribute : INetworkSerializable{
	public float moveSpeed = 7f;
	public float maxHealth = 3;

	//Define by Weapon
	public int maxBullet;
	public float damage;
	public float shootCd;

	public const float RotSpeed = 0.1f;
	public const float MoveFriction = 0.07f;

	public AvaterAttribute(){ }

	public AvaterAttribute(AvaterAttribute copy){
		moveSpeed = copy.moveSpeed;
		maxHealth = copy.maxHealth;
		maxBullet = copy.maxBullet;
		damage = copy.damage;
		shootCd = copy.shootCd;
	}


	public void Copy(AvaterAttribute copy){
		moveSpeed = copy.moveSpeed;
		maxHealth = copy.maxHealth;
		maxBullet = copy.maxBullet;
		damage = copy.damage;
		shootCd = copy.shootCd;
	}

	public void AddAttribute(AttributeType type, float value){
		switch(type){
			case AttributeType.MoveSpeed:
				moveSpeed += value;
				moveSpeed = Mathf.Max(moveSpeed, 0.1f);
				break;
			case AttributeType.MaxHealth:
				maxHealth += value;
				break;
			case AttributeType.MaxBullet:
				maxBullet += (int)value;
				break;
			case AttributeType.Damage:
				damage += value;
				break;
			case AttributeType.ShootCd:
				shootCd += value;
				break;
		}
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter{
		serializer.SerializeValue(ref moveSpeed);
		serializer.SerializeValue(ref maxHealth);
		serializer.SerializeValue(ref maxBullet);
	}
}