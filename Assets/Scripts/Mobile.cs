using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions;
using UnityEngine;

public class Mobile : MonoBehaviour, IMoving
{
    private const float RayCastEpsilon = 0.01f;

    public Vector2 BodySize = Vector2.one;

    public Vector2 BodyOffset = Vector2.zero;

    public float RailStickiness = 0.3f;

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

    public Vector2 BodyPosition
    {
        get { return (Vector2) transform.position + BodyOffset; }
        set { transform.position = value - BodyOffset; }
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
        _currentAcceleration = _currentVelocity = Vector2.zero;
    }

    public void CancelMovementAlongAxis( Vector2 axis )
    {
        CurrentVelocity -= CurrentVelocity.Dot( axis ) * axis;
        CurrentAcceleration -= CurrentAcceleration.Dot( axis ) * axis;
    }

    public void SetVerticalVelocity( float velocity )
    {
        _currentVelocity.y = velocity;
    }

    public void SetHorizontalVelocity( float velocity )
    {
        _currentVelocity.x = velocity;
    }

    public void ChangeHorizontalVelocity( float desiredVelocity, float inertia )
    {
        _currentVelocity.x =
            Mathf.SmoothDamp( _currentVelocity.x, desiredVelocity, ref _currentAcceleration.x, inertia );
    }

    public void ChangeVerticalVelocity( float desiredVelocity, float inertia )
    {
        _currentVelocity.y =
            Mathf.SmoothDamp( _currentVelocity.y, desiredVelocity, ref _currentAcceleration.y, inertia );
    }

    public void ChangeVelocity( Vector2 desiredVelocity, float inertia )
    {
        _currentVelocity = Vector2.SmoothDamp( _currentVelocity, desiredVelocity, ref _currentAcceleration, inertia,
            float.MaxValue, Time.deltaTime );
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
    private int _passThroughLayerMask;
    private int _wallLayerMask;
    private int _obstacleLayerMask;
    private ContactFilter2D _wallCollisionContactFilter2D;
    private readonly Collider2D[] _wallColliders = new Collider2D[ 1 ];
    private Collider2D _collisionCheckCollider;

    #endregion

    #region MOVE METHODS

    public void Move()
    {
        Move( Vector2.zero );
    }

    public void Move( Vector2 velocityBias )
    {
//        DebugExtension.DebugArrow( BodyPosition, velocityBias, Color.blue );
//        DebugExtension.DebugArrow( BodyPosition, CurrentVelocity, Color.red );
        var amount = ( CurrentVelocity + velocityBias ) * Time.deltaTime;

        var length = amount.magnitude;

        var direction = amount.normalized;

        var hit = Physics2D.CapsuleCast( BodyPosition, 0.6f * BodySize, CapsuleDirection2D.Vertical, 0, direction,
            length,
            _moveBlockingLayerMask );

        if ( hit.collider != null )
        {
            DebugExtension.DebugPoint( hit.point, Color.yellow, 0.1f, 1 );
            BodyPosition = hit.centroid;
            CancelHorizontalMovement();
        }
        else
        {
            transform.Translate( direction * length );
        }

        CheckWallCollisions();
    }

    private void CheckWallCollisions()
    {
        if ( _collisionCheckCollider.OverlapCollider( _wallCollisionContactFilter2D, _wallColliders ) > 0 )
        {
            var distance2D = _collisionCheckCollider.Distance( _wallColliders[ 0 ] );
            DebugExtension.DebugPoint( distance2D.pointA, Color.red, 0.1f, 1 );
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
        var layerMask = _groundLayerMask | _passThroughLayerMask;
        var frontHit = Physics2D.Raycast( BodyPosition + 0.25f * BodySize.x * Direction * Vector2.right, Vector2.down,
            0.5f * BodySize.y + RailStickiness, layerMask );
        var backHit = Physics2D.Raycast( BodyPosition - 0.25f * BodySize.x * Direction * Vector2.right, Vector2.down,
            0.5f * BodySize.y + RailStickiness, layerMask );

        if ( backHit.collider == null && frontHit.collider == null )
        {
            col = null;
            normal = Vector2.zero;
            return false;
        }

        var selectedHit = backHit;
        if ( frontHit.collider != null )
        {
            selectedHit = frontHit;
        }

        DebugExtension.DebugPoint( selectedHit.point, 0.1f, 1 );

        normal = selectedHit.normal;
        col = selectedHit.collider;

        if ( snap )
        {
            var tangent = normal.Perp();

            var distance = Mathf.Abs( tangent.PerpDot( BodyPosition - selectedHit.point ) );
            BodyPosition = BodyPosition - normal * ( distance - 0.5f * BodySize.y );
            CancelMovementAlongAxis( normal );
        }

        return true;
    }

    public bool CheckCeiling()
    {
        Vector2 normal;
        return CheckCeiling( out normal );
    }

    public bool CheckCeiling( out Vector2 normal )
    {
        var hit = Physics2D.Raycast( BodyPosition, Vector2.up, 0.5f * BodySize.y + RayCastEpsilon, _groundLayerMask );

        normal = hit.normal;
        if ( hit.collider != null )
        {
            DebugExtension.DebugPoint( hit.point, 0.1f, 1 );
            CancelVerticalMovement();
            return true;
        }

        return false;
    }

    public bool ProbeCeiling(out float distance, float maxDistance = 10)
    {
        var hit = Physics2D.Raycast( BodyPosition, Vector2.up, maxDistance, _groundLayerMask );
        
        if ( hit.collider != null )
        {
            distance = hit.point.y - ( BodyPosition.y + 0.5f * BodySize.y );
            return true;
        }

        distance = 0;
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
        var hit = Physics2D.Raycast( BodyPosition, Vector2.right * direction, 0.5f * BodySize.x + WallStickiness,
            _wallLayerMask );
        if ( hit.collider != null )
        {
            DebugExtension.DebugPoint( hit.point, 0.1f, 1 );
            // snap to wall
            BodyPosition = hit.point + hit.normal * 0.5f * BodySize.x;
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
        _passThroughLayerMask = 1 << LayerMask.NameToLayer( Layers.PassThrough );

        _moveBlockingLayerMask = _groundLayerMask |
                                 _wallLayerMask |
                                 _obstacleLayerMask;

        _wallCollisionContactFilter2D = new ContactFilter2D();
        _wallCollisionContactFilter2D.NoFilter();
        _wallCollisionContactFilter2D.SetLayerMask( _wallLayerMask |
                                                    _groundLayerMask |
                                                    _obstacleLayerMask );

        _collisionCheckCollider = GetComponentsInChildren<Collider2D>()
            .First( col => col.CompareTag( Tags.Collisionbox ) );
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
            _previousPositions.Enqueue( BodyPosition );
            while ( _previousPositions.Count > TrackPositions.Value )
            {
                _previousPositions.Dequeue();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        DebugExtension.DrawCapsule( (Vector3) BodyPosition + 0.5f * BodySize.y * Vector3.down,
            (Vector3) BodyPosition + 0.5f * BodySize.y * Vector3.up, Gizmos.color, 0.5f * BodySize.x );

        var top = BodyPosition + Vector2.up * ( 0.5f * BodySize.y + RayCastEpsilon );
        Gizmos.DrawLine( BodyPosition, top );
        Gizmos.DrawLine( top + 0.1f * Vector2.left, top + 0.1f * Vector2.right );

        var front = BodyPosition + Vector2.right * Direction * ( 0.5f * BodySize.x + WallStickiness );
        Gizmos.DrawLine( BodyPosition, front );
        Gizmos.DrawLine( front + 0.1f * Vector2.up, front + 0.1f * Vector2.down );
        Gizmos.DrawLine( front + 0.1f * Vector2.up, front + 0.1f * Vector2.down );

        var backBottom = BodyPosition - 0.25f * BodySize.x * Direction * Vector2.right;
        var frontBottom = BodyPosition + 0.25f * BodySize.x * Direction * Vector2.right;
        var down = Vector2.down * ( 0.5f * BodySize.y + RailStickiness );
        Gizmos.DrawLine( backBottom, backBottom + down );
        Gizmos.DrawLine( frontBottom, frontBottom + down );
        Gizmos.DrawLine( backBottom + down + 0.1f * Vector2.left, backBottom + down + 0.1f * Vector2.right );
        Gizmos.DrawLine( frontBottom + down + 0.1f * Vector2.left, frontBottom + down + 0.1f * Vector2.right );

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