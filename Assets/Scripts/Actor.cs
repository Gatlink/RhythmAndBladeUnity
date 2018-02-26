using System;
using ActorStates;
using Controllers;
using Gamelogic.Extensions;
using UnityEngine;

[ SelectionBase ]
public class Actor : GLMonoBehaviour
{
    [ Header( "Inputs" ) ]
    [ ReadOnly ]
    public float DesiredMovement;

    [ ReadOnly ]
    public bool DesiredJump;

    [ ReadOnly ]
    public bool DesiredAttack;

    [ ReadOnly ]
    public bool DesiredDash;

    [ Header( "State" ) ]
    [ ReadOnly ]
    public string StateName;

    public event Action<IActorState, IActorState> StateChangeEvent;

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

    [ Header( "Setup" ) ]
    public ActorControllerBase Controller;

    private IActorState _currentState;

    private PlayerSettings _playerSettings;

    public Mobile Mobile { get; private set; }
    
    public ActorHealth Health { get; private set; }

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

    private void ResetInputs()
    {
        DesiredMovement = 0;
        DesiredJump = false;
        DesiredAttack = false;
        DesiredDash = false;
    }

    private void OnStateChangeEvent( IActorState previousState, IActorState nextState )
    {
        var handler = StateChangeEvent;
        if ( handler != null ) handler( previousState, nextState );
    }
   
    #region UNITY MESSAGES

    private void Awake()
    {
        _playerSettings = PlayerSettings.Instance;
        
        Mobile = GetRequiredComponent<Mobile>();
        Mobile.BodyRadius = _playerSettings.BodyRadius;
        Mobile.RailStickiness = _playerSettings.RailStickiness;
        Mobile.WallStickiness = _playerSettings.WallStickiness;

        Health = GetRequiredComponent<ActorHealth>();
        Health.TotalHitCount = _playerSettings.InitialHitCount;
    }

    private void Start()
    {        
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
    }

    #endregion UNITY MESSAGES
}