using UnityEngine;

public class ClampedCurveAttribute : PropertyAttribute
{
    public readonly float MinX;

    public readonly float MaxX;

    public readonly float MinY;

    public readonly float MaxY;

    public ClampedCurveAttribute( float minX, float maxX, float minY, float maxY )
    {
        MinX = minX;
        MaxX = maxX;
        MinY = minY;
        MaxY = maxY;
    }

    public ClampedCurveAttribute() : this( 0, 1, 0, 1 )
    {
    }
}