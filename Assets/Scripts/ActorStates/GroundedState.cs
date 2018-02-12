using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public class GroundedState : ActorStateBase
    {
        public GroundedState( Actor actor ) : base( actor )
        {
        }

        public override void OnEnter()
        {
            Actor.DashCount = 1;
            Actor.AttackCount = 1;
        }

        public override IActorState Update()
        {
            Vector2 normal;
            if ( !Actor.CheckGround( out normal ) )
            {
                Debug.LogWarning( "Should not happen except during the very first frame" );
                return new FallState( Actor );
            }

            var desiredVelocity = Actor.DesiredMovement * PlayerSettings.GroundedMovementSpeed;
            Actor.UpdateDirection( desiredVelocity );

            // move along rail
            Vector3 tangent = normal.Rotate270();

            // update current velocity accounting inertia
            Actor.CurrentVelocity = Vector3.SmoothDamp( Actor.CurrentVelocity, tangent * desiredVelocity,
                ref Actor.CurrentAcceleration, PlayerSettings.GroundedMoveInertia );

            // default move
            Actor.Move( Actor.CurrentVelocity * Time.deltaTime );

            Actor.CheckWallCollisions();

            if ( !Actor.CheckGround() )
            {
                return new FallState( Actor );
            }

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

            return null;
        }
    }
}