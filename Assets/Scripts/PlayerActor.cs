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

    [ ReadOnly ]
    public bool DesiredBeatMode;

    [ ReadOnly ]
    public BeatManager.BeatAction DesiredBeatActions;

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

    public void AddAttackCooldown( float cooldown )
    {
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
        Collider2D hurtBox = null;
        Collider2D hitCollider = null;
        foreach ( var box in GetComponentsInChildren<Collider2D>()
            .Where( col => col.CompareTag( Tags.Hurtbox ) && col.enabled ) )
        {
            var hitCount = box.OverlapCollider( _hurtContactFilter2D, _colliderBuffer );
            if ( hitCount > 0 )
            {
                hitCollider = _colliderBuffer[ 0 ];
                hurtBox = box;
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
                var distance2D = Physics2D.Distance( hurtBox, hitCollider );
                var recoilStrength = harmfull.Recoil;
                return new HurtState( this, -recoilStrength * distance2D.normal );
            }
            else if ( !Health.IsAlive )
            {
                return new DeathState( this );
            }
        }

        return null;
    }

    private ContactFilter2D _hitContactFilter2D;

    public void DealDamage( uint hitId )
    {
        foreach ( var hitbox in GetComponentsInChildren<Collider2D>()
            .Where( col => col.CompareTag( Tags.Hitbox ) && col.enabled ) )
        {
#if UNITY_EDITOR
            GeometryUtils.DebugCollider( hitbox );
#endif
            var hitCount = hitbox.OverlapCollider( _hitContactFilter2D, _colliderBuffer );
            if ( hitCount > 0 )
            {
                for ( int i = 0; i < hitCount; i++ )
                {
                    var colliderHit = _colliderBuffer[ i ];
                    var destructible = colliderHit.GetInterfaceComponentInParent<IDestructible>();
                    if ( destructible != null )
                    {
                        destructible.Hit( new HitInfo( hitId, gameObject ) );
                    }
                }
            }
        }
    }

    protected override void ResetIntent()
    {
        DesiredMovement = 0;
        DesiredJump = false;
        DesiredAttack = false;
        DesiredDash = false;
        DesiredBeatMode = false;
        DesiredBeatActions = BeatManager.BeatAction.None;
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

        _hitContactFilter2D = new ContactFilter2D();
        _hitContactFilter2D.NoFilter();
        _hitContactFilter2D.SetLayerMask( 1 << LayerMask.NameToLayer( Layers.Destructible ) );
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