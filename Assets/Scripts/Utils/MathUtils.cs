using UnityEngine;

public static class MathUtils
{
    public const float NormalizationEpsilon = 9.99999974737875E-06f;

    public static float InvLerp( float a, float b, float v )
    {
        return Mathf.Clamp01( ( v - a ) / ( b - a ) );
    }

    public static Vector2 ClampedMagnitude( this Vector2 self, float max )
    {
        var magnitude = self.magnitude;
        if ( magnitude > max )
        {
            return max / magnitude * self;
        }

        return self;
    }
}