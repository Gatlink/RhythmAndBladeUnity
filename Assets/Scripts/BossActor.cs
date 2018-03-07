using ActorStates;
using ActorStates.Boss;
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
    public bool DesiredCharge;

    public Mobile Mobile { get; private set; }

    public ActorHealth Health { get; private set; }

    private bool _hurtThisFrame;
    private bool _canTransitionToHurt;
    private float _hurtDirection;

    public bool CheckAttack()
    {
        return DesiredAttack;
    }

    public bool CheckJumpAttack()
    {
        return DesiredJumpAttack;
    }

    public bool CheckCharge()
    {
        return DesiredCharge;
    }

    public bool CheckHurt()
    {
        return _hurtThisFrame && ( _canTransitionToHurt || !Health.IsAlive );
    }

    public void SetCanTransitionToHurt( bool state )
    {
        _canTransitionToHurt = state;
    }

    private void HitHandler( ActorHealth health, GameObject source )
    {
        _hurtThisFrame = true;
        _hurtDirection = Mathf.Sign( source.transform.position.x - Mobile.transform.position.x );
    }

    protected override void ResetIntent()
    {
        DesiredMovement = 0;
        DesiredAttack = false;
        DesiredJumpAttack = false;
        DesiredCharge = false;

        _hurtThisFrame = false;
    }

    protected override IActorState CreateInitialState()
    {
        return new FallState( this );
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
            TransitionToState( new HurtState( this, -_hurtDirection ) );
        }
    }
}