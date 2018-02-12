using System;
using System.Collections.Generic;
using System.Linq;
using ActorStates;
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
    public float Direction = 1;

    [ ReadOnly ]
    public int DashCount = 1;

    [ ReadOnly ]
    public float AttackCooldown;

    [ ReadOnly ]
    public float AttackCount = 1;

    [ ReadOnly ]
    public Vector3 CurrentVelocity;

    [ HideInInspector ]
    public Vector3 CurrentAcceleration;

    [ Header( "Setup" ) ]
    public ActorControllerBase Controller;

    // minimum movement to change direction
    private const float MovementEpsilon = 0.01f;

    public IActorState CurrentState { get; private set; }

    private PlayerSettings _playerSettings;

    private ContactFilter2D _wallCollisionContactFilter2D;

    private int _moveBlockingLayerMask;

    public bool CheckJump()
    {
        return DesiredJump;
    }

    public bool CheckDash()
    {
        return DesiredDash && DashCount > 0;
    }

    public bool CheckAttack( bool isCombo = false )
    {
        return DesiredAttack && ( isCombo || AttackCooldown <= 0 && AttackCount > 0 );
    }

    public bool CheckGround( bool snap = true )
    {
        Vector2 normal;
        return CheckGround( out normal, snap );
    }

    public bool CheckGround( out Vector2 normal, bool snap = true )
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
        return CheckWallProximity( direction, out normal );
    }

    public bool CheckWallProximity( float direction, out Vector2 normal )
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
            return true;
        }

        normal = Vector2.zero;
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
        _wallCollisionContactFilter2D = new ContactFilter2D()
        {
            layerMask = 1 << LayerMask.NameToLayer( Layers.Wall ) | 1 << LayerMask.NameToLayer( Layers.Destructible ),
            useLayerMask = true
        };
        _moveBlockingLayerMask = 1 << LayerMask.NameToLayer( Layers.Ground ) |
                                 1 << LayerMask.NameToLayer( Layers.Wall ) |
                                 1 << LayerMask.NameToLayer( Layers.Destructible );
    }

    private void Start()
    {
        _playerSettings = PlayerSettings.Instance;
        CurrentState = new FallState( this );
        CurrentState.OnEnter();
        StateName = CurrentState.Name;
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

        if ( CurrentState == null )
        {
            Debug.LogError( "Actor has no current state", this );
        }
        else
        {
            var nextState = CurrentState.Update();
            if ( nextState != null )
            {
                Debug.Log( string.Format( "Going from {0} to {1}", CurrentState.Name, nextState.Name ) );
                CurrentState.OnExit();
                OnStateChangeEvent( CurrentState, nextState );
                nextState.OnEnter();
                CurrentState = nextState;
                StateName = CurrentState.Name;
            }
        }

        ResetInputs();
        if ( AttackCooldown > 0 )
        {
            AttackCooldown = Mathf.Max( 0, AttackCooldown - Time.deltaTime );
        }
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