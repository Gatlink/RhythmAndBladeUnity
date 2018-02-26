using UnityEngine;

namespace ActorStates
{
    public class FallState : ActorStateBase
    {
        public FallState( Actor actor ) : base( actor )
        {
        }

        public override IActorState Update()
        {
            var desiredVelocity = Actor.DesiredMovement * PlayerSettings.FallMovementSpeed;

            Actor.UpdateDirection( desiredVelocity );

            var mob = Actor.Mobile;
            var velocity = mob.CurrentVelocity;
            var acceleration = mob.CurrentAcceleration;

            // update current horizontal velocity accounting inertia
            velocity.x = Mathf.SmoothDamp( velocity.x, desiredVelocity, ref acceleration.x,
                PlayerSettings.FallMoveInertia );

            // apply gravity
            velocity.y -= PlayerSettings.Gravity * Time.deltaTime;
            velocity.y = Mathf.Max( -PlayerSettings.MaxFallVelocity, velocity.y );

            mob.CurrentVelocity = velocity;
            mob.CurrentAcceleration = acceleration;

            // default move
            Actor.Mobile.Move();

            var harmfull = Actor.CheckDamages();
            if ( harmfull != null )
            {
                return new HurtState( Actor, harmfull );
            }

            Vector2 normal;
            if ( Actor.CheckWallProximity( Actor.Direction, out normal ) )
            {
                return new WallSlideState( Actor, normal );
            }

            if ( Actor.CheckGround() )
            {
                return new GroundedState( Actor );
            }

            if ( Actor.CheckDash() )
            {
                return new DashState( Actor );
            }

            if ( Actor.CheckAttack() )
            {
                return new AttackState( Actor );
            }

            return null;
        }
    }
}