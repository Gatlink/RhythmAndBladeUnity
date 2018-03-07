using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[DefaultExecutionOrder(-101)]
public class MovingObject : MonoBehaviour, IMoving
{
    public Vector2 Displacement = Vector2.up;
    public float Period = 1f;
    public float Phase;
    private Vector2 _initialPosition;

    public Vector2 CurrentVelocity
    {
        get
        {
            return Displacement / ( Period / 2 ) *
                   Mathf.Sign( 0.5f - Mathf.Repeat( Time.time + Phase, Period ) / Period );
        }
        set { throw new NotSupportedException( "Cannot set current velocity" ); }
    }

    public Vector2 CurrentAcceleration
    {
        get { return Vector2.zero; }
        set { throw new NotSupportedException( "Cannot set current acceleration" ); }
    }

    private void Start()
    {
        _initialPosition = transform.position;
    }

    private void Update()
    {
        transform.position = Vector2.Lerp( _initialPosition, _initialPosition + Displacement,
            Mathf.PingPong( Time.time + Phase, Period * 0.5f ) / ( Period * 0.5f ) );
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var position = EditorApplication.isPlaying ? _initialPosition : (Vector2) transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawLine( position, position + Displacement );
    }
#endif
}