﻿using System.Collections.Generic;
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
    public int DashCount;

    [ ReadOnly ]
    public float AttackCooldown;

    [ ReadOnly ]
    public Vector3 CurrentVelocity;

    [ HideInInspector ]
    public Vector3 CurrentAcceleration;

    [ Header( "Setup" ) ]
    public ActorControllerBase Controller;

    // minimum movement to change direction
    private const float MovementEpsilon = 0.01f;

    // wall angle threshold from 90deg
    private readonly float WallAngleThresholdCosine = Mathf.Cos( 45 * Mathf.Deg2Rad );

    private IActorState _currentState;

    private PlayerSettings _playerSettings;

    public bool CheckJump()
    {
        return DesiredJump;
    }

    public bool CheckDash()
    {
        return DesiredDash && DashCount > 0;
    }

    public bool CheckAttack()
    {
        return DesiredAttack && AttackCooldown <= 0;
    }

    public bool CheckGround()
    {
        RaycastHit2D hit;
        return CheckGround( out hit );
    }

    public bool CheckGround( out RaycastHit2D hit )
    {
        hit = Physics2D.Raycast( transform.position, Vector2.down,
            _playerSettings.BodyRadius + _playerSettings.RailStickiness, 1 << LayerMask.NameToLayer( "Rail" ) );

        return hit.collider != null;
    }
    
    public void UpdateDirection( float desiredVelocity )
    {
        if ( Mathf.Abs( desiredVelocity ) > MovementEpsilon )
        {
            Direction = Mathf.Sign( desiredVelocity );
        }
    }

    public void Move( bool snapGround = true, bool snapWalls = true )
    {
        var amount = CurrentVelocity * Time.deltaTime;
        var length = amount.magnitude;

        if ( length > MovementEpsilon )
        {
            var direction = amount / length;
            var hit = Physics2D.Raycast( transform.position, direction, length + _playerSettings.BodyRadius,
                1 << LayerMask.NameToLayer( "Rail" ) );
            if ( hit.collider != null )
            {
                length = hit.distance - _playerSettings.BodyRadius;
            }

            transform.Translate( direction * length );
        }

        if ( snapGround )
        {
            SnapToGround();
        }

        if ( snapWalls )
        {
            SnapToWalls();
        }
    }

    private void SnapToGround()
    {
        var hit = Physics2D.Raycast( transform.position, Vector2.down,
            _playerSettings.BodyRadius + _playerSettings.RailStickiness,
            1 << LayerMask.NameToLayer( "Rail" ) );
        if ( hit.collider != null )
        {
            transform.position = hit.point + Vector2.up * _playerSettings.BodyRadius;
            CurrentVelocity.y = 0;
            CurrentAcceleration.y = 0;
        }
    }

    private void SnapToWalls()
    {
        var hit = Physics2D.Raycast( transform.position, Vector2.right * Direction, _playerSettings.BodyRadius,
            1 << LayerMask.NameToLayer( "Rail" ) );
        if ( hit.collider != null && Vector2.Dot( hit.normal, Vector2.up ) <= WallAngleThresholdCosine )
        {
            transform.Translate( hit.normal * ( _playerSettings.BodyRadius - hit.distance ) );
            CurrentVelocity.x = 0;
            CurrentAcceleration.x = 0;
        }
    }

    private void ResetInputs()
    {
        DesiredMovement = 0;
        DesiredJump = false;
        DesiredAttack = false;
        DesiredDash = false;
    }

    #region UNITY MESSAGES

    private void Start()
    {
        _playerSettings = PlayerSettings.Instance;
        _currentState = new GroundedState( this );
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
                Debug.Log( string.Format( "Going from state {0} to state {1}", _currentState.Name, nextState.Name ) );
                _currentState.OnExit();
                nextState.OnEnter();
                _currentState = nextState;
                StateName = _currentState.Name;
            }
        }

        ResetInputs();
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