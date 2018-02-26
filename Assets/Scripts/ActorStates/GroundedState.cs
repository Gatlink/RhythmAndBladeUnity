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
            Actor.ResetDash();
            Actor.ResetJump();
            Actor.ResetAttack();
        }

        public override IActorState Update()
        {
            Vector2 normal;
            Collider2D collider;
            if ( !Actor.CheckGround( out collider, out normal ) )
            {
                Debug.LogWarning( "Should not happen except during the very first frame" );
                return new FallState( Actor );
            }

            var desiredVelocity = Actor.DesiredMovement * PlayerSettings.GroundedMovementSpeed;
            Actor.UpdateDirection( desiredVelocity );

            // move along rail
            var tangent = normal.Rotate270();

            var mob = Actor.Mobile;
            var velocity = mob.CurrentVelocity;
            var acceleration = mob.CurrentAcceleration;

            // update current velocity accounting inertia
            velocity = Vector2.SmoothDamp( velocity, tangent * desiredVelocity,
                ref acceleration, PlayerSettings.GroundedMoveInertia, float.MaxValue, Time.deltaTime );

            mob.CurrentVelocity = velocity;
            mob.CurrentAcceleration = acceleration;

            // add ground movement if there is any
            var groundMovement = Vector2.zero;
            if ( collider.gameObject.CompareTag( Tags.Moving ) )
            {
                var moving = collider.GetInterfaceComponentInParent<IMoving>();
                if ( moving == null )
                {
                    Debug.LogError( "IMoving component not found in " + collider, collider );
                }
                else
                {
                    groundMovement = moving.CurrentVelocity;
                }
            }

            // default move
            mob.Move( groundMovement );

            // check damages
            var harmfull = Actor.CheckDamages();
            if ( harmfull != null )
            {
                return new HurtState( Actor, harmfull );
            }

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