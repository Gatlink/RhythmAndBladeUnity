using ActorStates;
using Gamelogic.Extensions;
using UnityEngine;

public class PlayerActor : ActorBase<PlayerActor>
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

    protected override IActorState<PlayerActor> CreateInitialState()
    {
        return new FallState( this );
    }

    protected override void ResetIntent()
    {
        DesiredMovement = 0;
        DesiredJump = false;
        DesiredAttack = false;
        DesiredDash = false;
    }

    #region UNITY MESSAGES

    private void Awake()
    {
        _playerSettings = PlayerSettings.Instance;
        
        Mobile = GetRequiredComponent<Mobile>();
        Mobile.RailStickiness = _playerSettings.RailStickiness;
        Mobile.WallStickiness = _playerSettings.WallStickiness;

        Health = GetRequiredComponent<ActorHealth>();
        Health.TotalHitCount = _playerSettings.InitialHitCount;        
    }

    protected override void Update()
    {
        base.Update();
        if ( AttackCooldown > 0 )
        {
            AttackCooldown = Mathf.Max( 0, AttackCooldown - Time.deltaTime );
        }
    }

    #endregion UNITY MESSAGES
}