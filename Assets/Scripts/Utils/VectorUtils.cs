
using UnityEngine;

public static class VectorUtils
{
    public static Vector3 ClampToMagnitude(this Vector3 vector, float maxMagnitude)
    {
        if(vector.sqrMagnitude > maxMagnitude * maxMagnitude)
        {
            return vector.normalized * maxMagnitude;
        }
        else
        {
            return vector;
        }
    }
}
