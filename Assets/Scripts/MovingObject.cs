using System;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;

#endif

[ DefaultExecutionOrder( -101 ) ]
public class MovingObject : MonoBehaviour, IMoving
{
    public enum LoopType
    {
        PingPong,
        Repeat
    }

    public Vector2 Displacement = Vector2.up;

    public float LoopPeriod;

    public float Phase;

    public LoopType Loop;

    private Vector2 _initialPosition;

    public bool Enabled
    {
        get { return enabled; }
    }

    public Vector2 CurrentVelocity
    {
        get
        {
            var slope = Displacement / LoopPeriod;
            float sign = 1;
            if ( Loop == LoopType.PingPong )
            {
                sign = Mathf.Sign( 0.5f - Mathf.Repeat( Time.time + Phase, 2 * LoopPeriod ) / ( 2 * LoopPeriod ) );
            }

            return sign * slope;
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
        switch ( Loop )
        {
            case LoopType.PingPong:
                transform.position = Vector2.Lerp( _initialPosition, _initialPosition + Displacement,
                    Mathf.PingPong( Time.time + Phase, LoopPeriod ) / LoopPeriod );
                break;
            case LoopType.Repeat:
                transform.position = Vector2.Lerp( _initialPosition, _initialPosition + Displacement,
                    Mathf.Repeat( Time.time + Phase, LoopPeriod ) / LoopPeriod );
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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