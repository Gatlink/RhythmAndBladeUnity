using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[ DefaultExecutionOrder( -101 ) ]
public class MovingObject : MonoBehaviour, IMoving
{
    public Vector2 Displacement = Vector2.up;
    public float Period = 1f;
    public float Phase = 0f;
    private Vector3 _initialPosition;

    private void Start()
    {
        _initialPosition = transform.position;
    }

    public Vector3 CurrentVelocity
    {
        get
        {
            return Displacement / ( Period / 2 ) *
                   Mathf.Sign( 0.5f - Mathf.Repeat( Time.time + Phase, Period ) / Period );
        }
    }

    private void Update()
    {
        transform.position = Vector3.Lerp( _initialPosition, _initialPosition + (Vector3) Displacement,
            Mathf.PingPong( Time.time + Phase, Period * 0.5f ) / ( Period * 0.5f ) );
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var position = EditorApplication.isPlaying ? _initialPosition : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawLine( position, position + (Vector3) Displacement );
    }
#endif
}