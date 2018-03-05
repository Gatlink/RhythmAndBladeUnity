using UnityEngine;

public class CameraZoomConstraint : MonoBehaviour
{
    public AnimationCurve ZoomTransition = AnimationCurve.Constant( 0, 1, 1 );

    private Rail _cameraConstraint;

    public float GetZoom( float normalizedPosition )
    {
        return ZoomTransition.Evaluate( normalizedPosition );
    }

    private void OnValidate()
    {
        var keys = ZoomTransition.keys;
        for ( var i = 0; i < keys.Length; i++ )
        {
            keys[ i ].time = Mathf.Clamp01( keys[ i ].time );
        }

        ZoomTransition.keys = keys;
    }
}