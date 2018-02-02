using UnityEngine;

public class ClampedCurveAttribute : PropertyAttribute
{
    public float MinX = 0;

    public float MaxX = 1;

    public float MinY = 0;

    public float MaxY = 1;

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