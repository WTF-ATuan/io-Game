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
