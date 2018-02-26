using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions;
using UnityEngine;

public class MobileActor : MonoBehaviour, IMoving
{
    public float BodyRadius = 1;

    private int _moveBlockingLayerMask;

    private ContactFilter2D _wallCollisionContactFilter2D;

    [ ReadOnly, SerializeField ]
    private Vector2 _currentVelocity;

    [ ReadOnly, SerializeField ]
    private Vector2 _currentAcceleration;

    public Vector2 CurrentVelocity
    {
        get { return _currentVelocity; }
        set { _currentVelocity = value; }
    }

    public Vector2 CurrentAcceleration
    {
        get { return _currentAcceleration; }
        set { _currentAcceleration = value; }
    }

    public void Move()
    {
        Move( Vector2.zero );
    }

    public void Move( Vector2 velocityBias )
    {
        var amount = ( CurrentVelocity + velocityBias ) * Time.deltaTime;

        var length = amount.magnitude;

        var direction = amount.normalized;
        var hit = Physics2D.Raycast( transform.position, direction, length + BodyRadius, _moveBlockingLayerMask );
        if ( hit.collider != null )
        {
            length = hit.distance - BodyRadius;
            CurrentVelocity = Vector3.zero;
        }

        transform.Translate( direction * length );

        CheckWallCollisions();
    }

    private readonly Collider2D[] _wallColliders = new Collider2D[ 1 ];

    public bool CheckWallCollisions()
    {
        var thisCollider = GetComponent<Collider2D>();
        if ( thisCollider.OverlapCollider( _wallCollisionContactFilter2D, _wallColliders ) > 0 )
        {
            var distance2D = thisCollider.Distance( _wallColliders[ 0 ] );
            if ( distance2D.distance > 0 )
            {
//                Debug.LogError( "Should not be > 0" );
//                Debug.Log( string.Format( "{0} - {1}", distance2D.normal, distance2D.distance ) );
            }
            else
            {
                transform.Translate( distance2D.normal * distance2D.distance );
                CancelHorizontalMovement();
                return true;
            }
        }

        return false;
    }

    public void CancelHorizontalMovement()
    {
        _currentVelocity.x = _currentAcceleration.x = 0;
    }

    public void CancelVerticalMovement()
    {
        _currentVelocity.y = _currentAcceleration.y = 0;
    }

    public void CancelMovement()
    {
        _currentAcceleration = _currentAcceleration = Vector2.zero;
    }

    private void Awake()
    {
        _moveBlockingLayerMask = 1 << LayerMask.NameToLayer( Layers.Ground ) |
                                 1 << LayerMask.NameToLayer( Layers.Wall ) |
                                 1 << LayerMask.NameToLayer( Layers.Obstacle );

        _wallCollisionContactFilter2D = new ContactFilter2D();
        _wallCollisionContactFilter2D.NoFilter();
        _wallCollisionContactFilter2D.SetLayerMask( 1 << LayerMask.NameToLayer( Layers.Wall )
                                                    | 1 << LayerMask.NameToLayer( Layers.Obstacle ) );
    }

#if UNITY_EDITOR
    [ Header( "Gizmos" ) ]
    public OptionalInt TrackPositions = new OptionalInt();

    private readonly Queue<Vector3> _previousPositions = new Queue<Vector3>( 100 );


    private void LateUpdate()
    {
        if ( TrackPositions.UseValue )
        {
            _previousPositions.Enqueue( transform.position );
            while ( _previousPositions.Count > TrackPositions.Value )
            {
                _previousPositions.Dequeue();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere( transform.position, PlayerSettings.Instance.BodyRadius );
        if ( _previousPositions.Count > 1 )
        {
            var prev = _previousPositions.Peek();
            foreach ( var pos in _previousPositions.Skip( 1 ) )
            {
                Gizmos.DrawLine( prev, pos );
                prev = pos;
            }
        }
    }
#endif
}