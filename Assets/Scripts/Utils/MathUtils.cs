using UnityEngine;

public static class MathUtils
{
    public static float InvLerp( float a, float b, float v )
    {
        return Mathf.Clamp01( ( v - a ) / ( b - a ) );
    }
}