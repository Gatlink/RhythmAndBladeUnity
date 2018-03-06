﻿using ActorStates;
using ActorStates.Boss;

public class Boss1Animator : ActorAnimator<BossActor>
{
    public bool CanActorTransitionToHurt;

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
        else if ( next is DiveState )
        {
            Animator.SetTrigger( "Dive" );
        }
        else if ( next is StrikeGroundState )
        {
            Animator.SetTrigger( "StrikeGround" );
            CameraShake.ScreenShake( 1, 0.7f );
        }
        else if ( next is PrepareJump )
        {
            Animator.SetTrigger( "PrepareJump" );
        }
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        ((BossActor) Actor).SetCanTransitionToHurt( CanActorTransitionToHurt );
    }
}