using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public class GroundedState : ActorStateBase
    {
        public GroundedState( Actor actor ) : base( actor )
        {
        }

        public override IActorState Update()
        {
            RaycastHit2D hit;
            if ( !Actor.CheckGround( out hit ) )
            {
                return new FallState( Actor );
            }

            var desiredVelocity = Actor.DesiredMovement * PlayerSettings.GroundedMovementSpeed;
            Actor.UpdateDirection( desiredVelocity );

            // move along rail
            Vector3 tangent = hit.normal.Rotate270();

            // update current velocity accounting inertia
            Actor.CurrentVelocity = Vector3.SmoothDamp( Actor.CurrentVelocity, tangent * desiredVelocity,
                ref Actor.CurrentAcceleration, PlayerSettings.GroundedMoveInertia );

            // default move            
            Actor.Move();

            if ( Actor.CheckJump() )
            {
                return new JumpState( Actor );
            }

            if ( Actor.CheckDash() )
            {
                return new DashState( Actor );
            }

            if ( Actor.CheckAttack() )
            {
                return new AttackState( Actor );
            }

            if ( !Actor.CheckGround() )
            {
                return new FallState( Actor );
            }

            return null;
        }
    }
}