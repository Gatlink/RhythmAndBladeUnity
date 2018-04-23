using System;
using UnityEngine;
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

    private float _localTime;
    private Vector3 _lastPosition;

    public bool Enabled
    {
        get { return enabled; }
    }

    public Vector2 CurrentVelocity
    {
        get
        {
            if ( Loop == LoopType.PingPong )
            {
                return ( transform.position - _lastPosition ) / Time.deltaTime;
            }
            return Displacement / LoopPeriod;
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
        _localTime = Phase;
        _lastPosition = transform.position;
    }

    private void Update()
    {
        float time;
        switch ( Loop )
        {
            case LoopType.PingPong:
                time = Mathf.PingPong( _localTime, LoopPeriod );
                break;
            case LoopType.Repeat:
                time = Mathf.Repeat( _localTime, LoopPeriod );
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        transform.position = Vector2.Lerp( _initialPosition, _initialPosition + Displacement, time / LoopPeriod );

        // reset local time to 0 every two loops for ping pong
        _localTime = Mathf.Repeat( _localTime, LoopPeriod * 2 );
    }

    private void LateUpdate()
    {
        _localTime += Time.deltaTime;
        _lastPosition = transform.position;
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