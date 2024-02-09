using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff {
    public float AttackCD;
}

public class SummonEntity {
    public Action OnSpawn;
    public Action OnAttack;
    public Action OnDead;
}

public class Field:SummonEntity {
    
    public float LifeSec;
    public class FieldEvent {
        
        public float CD;
        public Action OnDo;

        public FieldEvent(Action onDo, float cd) {
            CD = cd;
            OnDo = onDo;
        }
    }
}

public class Bullet:SummonEntity {
    public float Damage;
    public float MoveSpeed;
    public bool ThroughTarget;
}

public class Pet:SummonEntity {
    public float Damage;
    public float MaxHealth;
    public float MoveSpeed; //0 = CantMove
    public float AttackCD;
}

public class SummonEntitySystem : MonoBehaviour {

}
