using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates.Boss
{
    public class GroundedState : BossActorStateBase
    {
        public GroundedState( BossActor actor ) : base( actor )
        {
        }

        public override IActorState Update()
        {
            var mob = Actor.Mobile;

            Vector2 normal;
            Collider2D collider;
            if ( !mob.CheckGround( out collider, out normal, ignorePassThrough: true ) )
            {
                Debug.LogWarning( "Should not happen except during the very first frame" );
                return new FallState( Actor );
            }

            var desiredVelocity = Actor.DesiredMovement * Settings.GroundedMovementSpeed;
            mob.UpdateDirection( desiredVelocity );

            // move along rail
            var tangent = normal.Rotate270();

            // update current velocity accounting inertia
            mob.ChangeVelocity( tangent * desiredVelocity, Settings.GroundedMoveInertia );

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

            if ( !mob.CheckGround() )
            {
                return new FallState( Actor );
            }

            if ( Actor.CheckJump() )
            {
                return new PrepareJumpState( Actor, false );
            }

            if ( Actor.CheckJumpAttack() )
            {
                return new PrepareJumpState( Actor, true );
            }

            if ( Actor.CheckCharge() )
            {
                return new ChargeAttackState( Actor );
            }

            if ( Actor.CheckAttack() )
            {
                return new AttackState( Actor );
            }

            return null;
        }
    }
}