using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Mathff
{
    public static float Angle(this Vector2 vector)
    {
        float radians = Mathf.Atan2(vector.y, vector.x);
        float degrees = radians * Mathf.Rad2Deg;
        return degrees;
    }
    
    public static Vector2 ToVec2(this float ang)
    {
        float radians = ang * Mathf.Deg2Rad; // 将角度转换为弧度
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }
    
    public static Vector2Int ToVec2Int(this Vector3 vec){
        return new Vector2Int(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y));
    }
    
    public static float AngleBetween(this float angle1, float angle2)
    {
        float difference = angle2 - angle1;
        while (difference < -180) difference += 360;
        while (difference > 180) difference -= 360;
        return difference;
    }
    
    public static float RotateTowards(this float currentAngle, float targetAngle, float maxRotation)
    {
        float angleDifference = AngleBetween(currentAngle, targetAngle);
        if (Mathf.Abs(angleDifference) <= maxRotation) return targetAngle;
        return currentAngle + Mathf.Sign(angleDifference) * maxRotation;
    }

}
