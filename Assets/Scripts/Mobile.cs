using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions;
using UnityEngine;

public class Mobile : MonoBehaviour, IMoving
{
    private const float RayCastEpsilon = 0.01f;
    
    [ HideInInspector ]
    public float BodyRadius = 1;

    [ HideInInspector ]
    public float RailStickiness = 0.2f;

    [ HideInInspector ]
    public float WallStickiness = 0.2f;

    [ ReadOnly ]
    public float Direction = 1;

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

    // minimum movement to change direction
    private const float MovementEpsilon = 0.01f;

    public void UpdateDirection( float direction )
    {
        if ( Mathf.Abs( direction ) > MovementEpsilon )
        {
            Direction = Mathf.Sign( direction );
        }
    }

    #region CACHING

    private int _moveBlockingLayerMask;
    private int _groundLayerMask;
    private int _wallLayerMask;
    private int _obstacleLayerMask;
    private ContactFilter2D _wallCollisionContactFilter2D;
    private readonly Collider2D[] _wallColliders = new Collider2D[ 1 ];

    #endregion

    #region MOVE METHODS

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
            CancelHorizontalMovement();
            //CurrentVelocity = Vector3.zero;
        }

        transform.Translate( direction * length );

        CheckWallCollisions();
    }

    private void CheckWallCollisions()
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
            }
        }
    }

    #endregion

    #region COLLISION CHECKS

    public bool CheckGround( bool snap = true )
    {
        Vector2 normal;
        Collider2D col;
        return CheckGround( out col, out normal, snap );
    }

    public bool CheckGround( out Vector2 normal, bool snap = true )
    {
        Collider2D col;
        return CheckGround( out col, out normal, snap );
    }

    public bool CheckGround( out Collider2D col, out Vector2 normal, bool snap = true )
    {
        var frontHit = Physics2D.Raycast( transform.position, Vector2.down, BodyRadius + RailStickiness,
            _groundLayerMask );
        var backHit = Physics2D.Raycast( transform.position - 0.5f * BodyRadius * Direction * Vector3.right,
            Vector2.down, BodyRadius + RailStickiness, _groundLayerMask );

        var selectedHit = backHit;
        if ( frontHit.collider != null )
        {
            selectedHit = frontHit;
        }

        var grounded = selectedHit.collider != null;
        if ( snap && grounded )
        {
            transform.position = transform.position.WithY( selectedHit.point.y + BodyRadius );
            CancelVerticalMovement();
        }

        normal = selectedHit.normal;
        col = selectedHit.collider;
        return grounded;
    }

    public bool CheckCeiling()
    {
        Vector2 normal;
        return CheckCeiling( out normal );
    }

    public bool CheckCeiling( out Vector2 normal )
    {
        var hit = Physics2D.Raycast( transform.position, Vector2.up, BodyRadius + RayCastEpsilon, _groundLayerMask );
        normal = hit.normal;
        if ( hit.collider != null )
        {
            CancelVerticalMovement();
            return true;
        }
        return false;
    }

    public bool CheckWallProximity( float direction )
    {
        Vector2 normal;
        Collider2D col;
        return CheckWallProximity( direction, out normal, out col );
    }

    public bool CheckWallProximity( float direction, out Vector2 normal )
    {
        Collider2D col;
        return CheckWallProximity( direction, out normal, out col );
    }

    public bool CheckWallProximity( float direction, out Vector2 normal, out Collider2D col )
    {
        var hit = Physics2D.Raycast( transform.position, Vector2.right * direction, BodyRadius + WallStickiness,
            _wallLayerMask );
        if ( hit.collider != null )
        {
            // snap to wall
            transform.position = hit.point + hit.normal * BodyRadius;
            CancelHorizontalMovement();
            normal = hit.normal;
            col = hit.collider;
            return true;
        }

        normal = Vector2.zero;
        col = null;
        return false;
    }

    #endregion

    #region UNITY MESSAGES
    
    private void Awake()
    {
        _groundLayerMask = 1 << LayerMask.NameToLayer( Layers.Ground );
        _wallLayerMask = 1 << LayerMask.NameToLayer( Layers.Wall );
        _obstacleLayerMask = 1 << LayerMask.NameToLayer( Layers.Obstacle );

        _moveBlockingLayerMask = _groundLayerMask |
                                 _wallLayerMask |
                                 _obstacleLayerMask;

        _wallCollisionContactFilter2D = new ContactFilter2D();
        _wallCollisionContactFilter2D.NoFilter();
        _wallCollisionContactFilter2D.SetLayerMask( _wallLayerMask |
                                                    _obstacleLayerMask );
    }
    
    #endregion

    #region GIZMOS

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

    #endregion
}