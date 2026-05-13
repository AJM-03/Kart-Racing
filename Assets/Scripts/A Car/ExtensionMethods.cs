using UnityEngine;

public static class ExtensionMethods
{
    // Remaps a value between two points to a value between two other points
    // Eg: Remapping 0.5 from a scale of 0-1 to a scale of 0-2 would give you 1
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }


    // Rounds a float
    public static float Round(this float f, int decimalPlaces = 2)
    {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++)
        {
            multiplier *= 10f;
        }
        return Mathf.Round(f * multiplier) / multiplier;
    }


    // Rounds a Vector 3
    public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 2)
    {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++)
        {
            multiplier *= 10f;
        }
        return new Vector3(
            Mathf.Round(vector3.x * multiplier) / multiplier,
            Mathf.Round(vector3.y * multiplier) / multiplier,
            Mathf.Round(vector3.z * multiplier) / multiplier);
    }


    // Converts angles from 0–360 to -180–180
    public static float NormalizeAngle(this float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}