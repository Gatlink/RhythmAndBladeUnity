using System;
using System.Collections.Generic;
using System.Linq;
using ActorStates;
using Controllers;
using Gamelogic.Extensions;
using UnityEngine;

[ SelectionBase ]
public class Actor : GLMonoBehaviour
{
    #region ACTOR INPUTS

    [ Header( "Inputs" ) ]
    [ ReadOnly ]
    public float DesiredMovement;

    [ ReadOnly ]
    public bool DesiredJump;

    [ ReadOnly ]
    public bool DesiredAttack;

    [ ReadOnly ]
    public bool DesiredDash;

    #endregion

    [ Header( "State" ) ]
    [ ReadOnly ]
    public string StateName;

    [ ReadOnly ]
    public int HitCount = 3;

    [ ReadOnly ]
    public float Direction = 1;

    [ ReadOnly ]
    [ SerializeField ]
    private int _dashCount = 1;

    [ SerializeField ]
    [ ReadOnly ]
    private int _jumpCount = 1;

    [ ReadOnly ]
    public float AttackCooldown;

    [ SerializeField ]
    [ ReadOnly ]
    private float _attackCount = 1;

    [ ReadOnly ]
    public Vector3 CurrentVelocity;

    [ HideInInspector ]
    public Vector3 CurrentAcceleration;

    [ Header( "Setup" ) ]
    public ActorControllerBase Controller;

    // minimum movement to change direction
    private const float MovementEpsilon = 0.01f;

    private IActorState _currentState;

    private PlayerSettings _playerSettings;

    private ContactFilter2D _wallCollisionContactFilter2D;

    private int _moveBlockingLayerMask;

    private readonly Collider2D[] _colliderBuffer = new Collider2D[ 5 ];

    private ContactFilter2D _hurtContactFilter2D;

    public bool CheckJump()
    {
        return DesiredJump && _jumpCount > 0;
    }

    public void ConsumeJump()
    {
        _jumpCount--;
    }

    public void ResetJump()
    {
        _jumpCount = 1;
    }

    public void ConsumeDash()
    {
        _dashCount--;
    }

    public void ResetDash()
    {
        _dashCount = 1;
    }

    public bool CheckDash()
    {
        return DesiredDash && _dashCount > 0;
    }

    public void ConsumeAttack( float cooldown )
    {
        _attackCount--;
        AttackCooldown = cooldown;
    }

    public void ResetAttack()
    {
        _attackCount = 1;
    }

    public bool CheckAttack( bool isCombo = false )
    {
        return DesiredAttack && ( isCombo || AttackCooldown <= 0 && _attackCount > 0 );
    }

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

    public bool CheckGround( out Collider2D collider2D, out Vector2 normal, bool snap = true )
    {
        var frontHit = Physics2D.Raycast( transform.position, Vector2.down,
            _playerSettings.BodyRadius + _playerSettings.RailStickiness, 1 << LayerMask.NameToLayer( Layers.Ground ) );
        var backHit = Physics2D.Raycast(
            transform.position - 0.5f * _playerSettings.BodyRadius * Direction * Vector3.right,
            Vector2.down, _playerSettings.BodyRadius + _playerSettings.RailStickiness,
            1 << LayerMask.NameToLayer( Layers.Ground ) );

        var selectedHit = backHit;
        if ( frontHit.collider != null )
        {
            selectedHit = frontHit;
        }

        var grounded = selectedHit.collider != null;
        if ( snap && grounded )
        {
            transform.position = transform.position.WithY( selectedHit.point.y + _playerSettings.BodyRadius );
            CurrentVelocity.y = 0;
            CurrentAcceleration.y = 0;
        }

        normal = selectedHit.normal;
        collider2D = selectedHit.collider;
        return grounded;
    }

    public bool CheckCeiling()
    {
        Vector2 normal;
        return CheckCeiling( out normal );
    }

    public bool CheckCeiling( out Vector2 normal )
    {
        var hit = Physics2D.Raycast( transform.position, Vector2.up, _playerSettings.BodyRadius,
            1 << LayerMask.NameToLayer( Layers.Ground ) );
        normal = hit.normal;
        return hit.collider != null;
    }

    public void UpdateDirection( float desiredVelocity )
    {
        if ( Mathf.Abs( desiredVelocity ) > MovementEpsilon )
        {
            Direction = Mathf.Sign( desiredVelocity );
        }
    }

    public void Move( Vector2 amount )
    {
        var length = amount.magnitude;

        var direction = amount.normalized;
        var hit = Physics2D.Raycast( transform.position, direction, length + _playerSettings.BodyRadius,
            _moveBlockingLayerMask );
        if ( hit.collider != null )
        {
            length = hit.distance - _playerSettings.BodyRadius;
            CurrentVelocity = Vector3.zero;
        }

        transform.Translate( direction * length );
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

    public bool CheckWallProximity( float direction, out Vector2 normal, out Collider2D collider )
    {
        var hit = Physics2D.Raycast( transform.position, Vector2.right * direction,
            _playerSettings.BodyRadius + _playerSettings.WallStickiness, 1 << LayerMask.NameToLayer( Layers.Wall ) );
        if ( hit.collider != null )
        {
            // snap to wall
            transform.position = hit.point + hit.normal * _playerSettings.BodyRadius;
            CurrentVelocity.x = 0;
            CurrentAcceleration.x = 0;
            normal = hit.normal;
            collider = hit.collider;
            return true;
        }

        normal = Vector2.zero;
        collider = null;
        return false;
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
                transform.Translate( ( distance2D.normal * distance2D.distance ) );
                CurrentVelocity.x = 0;
                CurrentAcceleration.x = 0;
                return true;
            }
        }

        return false;
    }

    public Collider2D CheckDamages()
    {
        foreach ( var hurtbox in GetComponentsInChildren<Collider2D>()
            .Where( col => col.CompareTag( Tags.Hurtbox ) && col.enabled ) )
        {
            var hitCount = hurtbox.OverlapCollider( _hurtContactFilter2D, _colliderBuffer );
            if ( hitCount > 0 )
            {
                return _colliderBuffer[ 0 ];
            }
        }

        return null;
    }

    public void AccountDamages( int amount )
    {
        HitCount = Mathf.Max( 0, HitCount - amount );
    }

    private void ResetInputs()
    {
        DesiredMovement = 0;
        DesiredJump = false;
        DesiredAttack = false;
        DesiredDash = false;
    }

    public event Action<IActorState, IActorState> StateChangeEvent;

    private void OnStateChangeEvent( IActorState previousState, IActorState nextState )
    {
        var handler = StateChangeEvent;
        if ( handler != null ) handler( previousState, nextState );
    }

    #region UNITY MESSAGES

    private void Awake()
    {
        _wallCollisionContactFilter2D = new ContactFilter2D();
        _wallCollisionContactFilter2D.NoFilter();
        _wallCollisionContactFilter2D.SetLayerMask( 1 << LayerMask.NameToLayer( Layers.Wall )
                                                    | 1 << LayerMask.NameToLayer( Layers.Obstacle ) );

        _moveBlockingLayerMask = 1 << LayerMask.NameToLayer( Layers.Ground ) |
                                 1 << LayerMask.NameToLayer( Layers.Wall ) |
                                 1 << LayerMask.NameToLayer( Layers.Obstacle );

        _hurtContactFilter2D = new ContactFilter2D();
        _hurtContactFilter2D.NoFilter();
        _hurtContactFilter2D.SetLayerMask( 1 << LayerMask.NameToLayer( Layers.Harmfull ) );
    }

    private void Start()
    {
        _playerSettings = PlayerSettings.Instance;
        HitCount = _playerSettings.InitialHitCount;
        _currentState = new FallState( this );
        _currentState.OnEnter();
        StateName = _currentState.Name;
    }

    private void Update()
    {
        // update inputs
        if ( Controller == null )
        {
            Debug.LogError( "Actor has no controller", this );
        }
        else
        {
            Controller.UpdateActorIntent( this );
        }

        if ( _currentState == null )
        {
            Debug.LogError( "Actor has no current state", this );
        }
        else
        {
            var nextState = _currentState.Update();
            if ( nextState != null )
            {
                Debug.Log( string.Format( "Going from {0} to {1}", _currentState.Name, nextState.Name ) );
                _currentState.OnExit();
                OnStateChangeEvent( _currentState, nextState );
                nextState.OnEnter();
                _currentState = nextState;
                StateName = _currentState.Name;
            }
        }

        ResetInputs();
        if ( AttackCooldown > 0 )
        {
            AttackCooldown = Mathf.Max( 0, AttackCooldown - Time.deltaTime );
        }

//        if ( HitCount <= 0 )
//        {
//            // todo die
//            Debug.Log( this + " died", this );
//        }
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

    #endregion UNITY MESSAGES
}