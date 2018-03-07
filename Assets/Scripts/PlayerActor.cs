using System.Linq;
using ActorStates;
using ActorStates.Player;
using Gamelogic.Extensions;
using UnityEngine;
using FallState = ActorStates.Player.FallState;

[ DefaultExecutionOrder( 101 ) ]
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

    protected override IActorState CreateInitialState()
    {
        return new FallState( this );
    }

    private readonly Collider2D[] _colliderBuffer = new Collider2D[ 5 ];

    private ContactFilter2D _hurtContactFilter2D;

    public IActorState CheckDamages()
    {
        Collider2D hitCollider = null;
        foreach ( var hurtbox in GetComponentsInChildren<Collider2D>()
            .Where( col => col.CompareTag( Tags.Hurtbox ) && col.enabled ) )
        {
            var hitCount = hurtbox.OverlapCollider( _hurtContactFilter2D, _colliderBuffer );
            if ( hitCount > 0 )
            {
                hitCollider = _colliderBuffer[ 0 ];
                break;
            }
        }

        if ( hitCollider != null )
        {
            var harmfull = hitCollider.GetInterfaceComponentInParent<IHarmfull>();
            var source = harmfull.GameObject;

            Health.AccountDamages( harmfull.Damage, source );

            if ( !harmfull.PassiveHurt )
            {
                var recoilStrength = harmfull.Recoil;
                return new HurtState( this, source, recoilStrength );
            }
        }

        return null;
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

        Health = GetRequiredComponent<ActorHealth>();
        Health.TotalHitCount = _playerSettings.InitialHitCount;

        _hurtContactFilter2D = new ContactFilter2D();
        _hurtContactFilter2D.NoFilter();
        _hurtContactFilter2D.SetLayerMask( 1 << LayerMask.NameToLayer( Layers.Harmfull ) );
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