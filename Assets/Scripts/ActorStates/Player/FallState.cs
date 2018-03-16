using UnityEngine;

namespace ActorStates.Player
{
    public class FallState : PlayerActorStateBase
    {
        public FallState( PlayerActor actor ) : base( actor )
        {
        }

        public override IActorState Update()
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
            mob.Move();

            var harmfull = Actor.CheckDamages();
            if ( harmfull != null )
            {
                return harmfull;
            }

            if ( !Actor.GetComponent<ActorHealth>().IsAlive )
            {
                return new DeathState( Actor );
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