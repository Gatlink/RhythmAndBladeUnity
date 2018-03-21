using System.Linq;
using ActorStates;
using ActorStates.Boss;
using Controllers;
using Gamelogic.Extensions;
using UnityEngine;

public class BossActor : ActorBase<BossActor>
{
    [ Header( "Inputs" ) ]
    [ ReadOnly ]
    public float DesiredMovement;

    [ ReadOnly ]
    public bool DesiredAttack;

    [ ReadOnly ]
    public bool DesiredJumpAttack;

    [ ReadOnly ]
    public bool DesiredJump;

    [ ReadOnly ]
    public bool DesiredCharge;

    [ ReadOnly ]
    public float DesiredJumpMovement;

    public Mobile Mobile { get; private set; }

    public ActorHealth Health { get; private set; }

    private bool _hurtThisFrame;
    private bool _canTransitionToHurt;
    private float _hurtDirection;
    private bool _nextHurtIsCritical;

    public bool CheckAttack()
    {
        return DesiredAttack;
    }

    public bool CheckJumpAttack()
    {
        return DesiredJumpAttack;
    }

    public bool CheckJump()
    {
        return DesiredJump;
    }

    public bool CheckCharge()
    {
        return DesiredCharge;
    }

    public bool CheckHurt()
    {
        return _hurtThisFrame && ( _canTransitionToHurt
                                   || !Health.IsAlive
                                   || _nextHurtIsCritical );
    }

    public void SetCanTransitionToHurt( bool state )
    {
        _canTransitionToHurt = state;
    }

    public void SetNextHurtIsCritical()
    {
        _nextHurtIsCritical = true;
    }

    private void HitHandler( ActorHealth health, GameObject source )
    {
        _hurtThisFrame = true;
        _hurtDirection = -Mathf.Sign( source.transform.position.x - Mobile.transform.position.x );
    }

    protected override IActorState CreateInitialState()
    {
        return new FallState( this );
    }

    protected override void Update()
    {
        _hurtThisFrame = false;
        base.Update();
    }

    private void Awake()
    {
        Mobile = GetRequiredComponent<Mobile>();

        Health = GetRequiredComponent<ActorHealth>();
        Health.HitEvent += HitHandler;
    }

    private void LateUpdate()
    {
        if ( CheckHurt() )
        {
            if ( _nextHurtIsCritical )
            {
                _nextHurtIsCritical = false;
                TransitionToState( new CriticalHurtState( this, _hurtDirection ) );
            }
            else
            {
                TransitionToState( new HurtState( this, _hurtDirection ) );
            }
        }
#if UNITY_EDITOR
        foreach ( var hitbox in GetComponentsInChildren<Collider2D>().Where( col =>
            col.enabled && col.gameObject.layer == LayerMask.NameToLayer( Layers.Harmfull ) ) )
        {
            GeometryUtils.DebugCollider( hitbox );
        }
#endif
    }
}