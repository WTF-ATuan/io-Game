using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    private Action OnDead;

    public void Setup(Vector2 genPos, float angle, float moveSec, float maxDis, Action onDead) {
        transform.position = genPos;
        transform.eulerAngles = new Vector3(0, 0, angle);
        OnDead = onDead;
//(Vector2)transform.forward
        transform.DOMove((genPos + angle.ToVec2() * maxDis), moveSec).SetEase(Ease.Linear).OnComplete(() => {
            OnDead.Invoke();
        });
    }
}
