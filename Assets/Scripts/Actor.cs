using System;
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

    public event Action<IActorState, IActorState> StateChangeEvent;

    [ ReadOnly ]
    public int TotalHitCount = 3;

    [ ReadOnly ]
    public int CurrentHitCount = 3;

    public delegate void ActorHitHandler( Actor actor );

    public event ActorHitHandler HitEvent;
   
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

    private readonly Collider2D[] _colliderBuffer = new Collider2D[ 5 ];

    private ContactFilter2D _hurtContactFilter2D;

    public MobileActor Mobile { get; private set; }

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
        CurrentHitCount = Mathf.Max( 0, CurrentHitCount - amount );
        OnHitEvent( this );
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

    private void OnHitEvent( Actor actor )
    {
        var handler = HitEvent;
        if ( handler != null ) handler( actor );
    }

    #region UNITY MESSAGES

    private void Awake()
    {
        _playerSettings = PlayerSettings.Instance;
        TotalHitCount = CurrentHitCount = _playerSettings.InitialHitCount;

        _hurtContactFilter2D = new ContactFilter2D();
        _hurtContactFilter2D.NoFilter();
        _hurtContactFilter2D.SetLayerMask( 1 << LayerMask.NameToLayer( Layers.Harmfull ) );
    }

    private void Start()
    {
        Mobile = GetRequiredComponent<MobileActor>();
        Mobile.BodyRadius = _playerSettings.BodyRadius;
        Mobile.RailStickiness = _playerSettings.RailStickiness;
        Mobile.WallStickiness = _playerSettings.WallStickiness;

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

        if ( CurrentHitCount <= 0 )
        {
            // todo die
            Debug.Log( this + " died", this );
        }
    }

    #endregion UNITY MESSAGES
}