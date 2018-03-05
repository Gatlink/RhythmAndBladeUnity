using ActorStates;
using ActorStates.Boss;

public class Boss1Animator : ActorAnimator<BossActor>
{
    protected override void StateChangeHandler( IActorState previous, IActorState next )
    {
        if ( next is FallState )
        {
            Animator.SetTrigger( "Fall" );
        }
        else if ( next is GroundedState )
        {
            Animator.SetTrigger( "Ground" );
        }
        else if ( next is JumpAttackState )
        {
            Animator.SetTrigger( "JumpAttack" );
        }
        else if ( next is AttackState )
        {
            var attackState = (AttackState) next;
            Animator.SetInteger( "Combo", attackState.ComboCount );
            Animator.SetTrigger( "Attack" );
        }
        else if ( next is HurtState )
        {
            Animator.SetTrigger( "Hurt" );
        }
        else if ( next is DeathState )
        {
            Animator.SetTrigger( "Death" );
        }
    }

    public void TriggetScreenShake()
    {
        CameraShake.ScreenShake( 1, 0.7f );
    }
}