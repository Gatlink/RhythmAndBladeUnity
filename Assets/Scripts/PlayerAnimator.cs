using ActorStates;
using ActorStates.Player;

public class PlayerAnimator : ActorAnimator<PlayerActor>
{
    protected override void StateChangeHandler( IActorState previous, IActorState next )
    {
        if ( next is JumpState )
        {
            Animator.SetTrigger( "Jump" );
        }
        else if ( next is DashState )
        {
            Animator.SetTrigger( "Dash" );
        }
        else if ( next is WallSlideState )
        {
            Animator.SetTrigger( "Slide" );
        }
        else if ( next is FallState )
        {
            Animator.SetTrigger( "Fall" );
        }
        else if ( next is GroundedState )
        {
            Animator.SetTrigger( "Ground" );
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
    }
}