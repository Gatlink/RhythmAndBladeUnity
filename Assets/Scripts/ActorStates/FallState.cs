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

            // update current horizontal velocity accounting inertia
            Actor.CurrentVelocity.x = Mathf.SmoothDamp( Actor.CurrentVelocity.x, desiredVelocity,
                ref Actor.CurrentAcceleration.x, PlayerSettings.FallMoveInertia );

            // apply gravity
            Actor.CurrentVelocity.y -= PlayerSettings.Gravity * Time.deltaTime;
            Actor.CurrentVelocity.y = Mathf.Max( -PlayerSettings.MaxFallVelocity, Actor.CurrentVelocity.y );

            // default move
            Actor.Move( Actor.CurrentVelocity * Time.deltaTime );

            Actor.CheckWalls();

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