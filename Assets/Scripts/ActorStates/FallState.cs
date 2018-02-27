using UnityEngine;

namespace ActorStates
{
    public class FallState : PlayerActorStateBase
    {
        public FallState( PlayerActor actor ) : base( actor )
        {
        }

        public override IActorState<PlayerActor> Update()
        {
            var desiredVelocity = Actor.DesiredMovement * PlayerSettings.FallMovementSpeed;

            var mob = Actor.Mobile;
            mob.UpdateDirection( desiredVelocity );

            // update current horizontal velocity accounting inertia
            mob.ChangeHorizontalVelocity( desiredVelocity, PlayerSettings.FallMoveInertia );
            
            // apply gravity
            var verticalVelocity = mob.CurrentVelocity.y - PlayerSettings.Gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max( verticalVelocity, -PlayerSettings.MaxFallVelocity );
            mob.SetVerticalVelocity( verticalVelocity  );

            // default move
            Actor.Mobile.Move();

            var harmfull = Actor.Health.CheckDamages();
            if ( harmfull != null )
            {
                return new HurtState( Actor, harmfull );
            }

            Vector2 normal;
            if ( mob.CheckWallProximity( mob.Direction, out normal ) )
            {
                return new WallSlideState( Actor, normal );
            }

            if ( mob.CheckGround() )
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