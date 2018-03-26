using System.Linq;
using System.Runtime.InteropServices;
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

    private bool _hurtPending;
    private bool _criticalHurtPending;
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

    public IActorState GetHurtState()
    {
        if ( !_hurtPending ) return null;        
        _hurtPending = false;
        
        if ( !_canTransitionToHurt && Health.IsAlive && !_criticalHurtPending ) return null;

        if ( !_criticalHurtPending ) return new HurtState( this, _hurtDirection );        
        _criticalHurtPending = false;
        
        return new CriticalHurtState( this, _hurtDirection );
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
        _hurtPending = true;
        _hurtDirection = -Mathf.Sign( source.transform.position.x - Mobile.transform.position.x );
        if ( _nextHurtIsCritical )
        {
            _nextHurtIsCritical = false;
            _criticalHurtPending = true;
        }
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

#if UNITY_EDITOR
    private void LateUpdate()
    {
        foreach ( var hitbox in GetComponentsInChildren<Collider2D>().Where( col =>
            col.enabled && col.gameObject.layer == LayerMask.NameToLayer( Layers.Harmfull ) ) )
        {
            GeometryUtils.DebugCollider( hitbox );
        }
    }
#endif
}