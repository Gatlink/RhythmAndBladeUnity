using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates.Player
{
    public class GroundedState : PlayerActorStateBase
    {
        public GroundedState( PlayerActor actor ) : base( actor )
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
            var mob = Actor.Mobile;
            
            Vector2 normal;
            Collider2D collider;
            if ( !mob.CheckGround( out collider, out normal ) )
            {
                Debug.LogWarning( "Should not happen except during the very first frame" );
                return new FallState( Actor );
            }

            var desiredVelocity = Actor.DesiredMovement * PlayerSettings.GroundedMovementSpeed;
            mob.UpdateDirection( desiredVelocity );

            // move along rail
            var tangent = normal.Rotate270();

            // update current velocity accounting inertia
            mob.ChangeVelocity( tangent * desiredVelocity, PlayerSettings.GroundedMoveInertia );

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
                return harmfull;
            }

            if ( !mob.CheckGround() )
            {
                return new FallState( Actor );
            }

            if ( Actor.CheckJump() )
            {
                return new JumpState( Actor, PlayerSettings.NormalJump );
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