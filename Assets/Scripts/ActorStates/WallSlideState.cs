using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public class WallSlideState : ActorStateBase
    {
        private readonly Vector2 _wallDirection;

        public WallSlideState( Actor actor, Vector2 wallNormal ) : base( actor )
        {
            _wallDirection = wallNormal;
        }

        public override void OnEnter()
        {
            Actor.CurrentVelocity = Vector3.zero;
        }

        public override IActorState Update()
        {
            Actor.UpdateDirection( _wallDirection.Dot( Vector2.right ) );

            // update horizontal velocity is 0
            Actor.CurrentVelocity.x = 0;

            // apply gravity
            Actor.CurrentVelocity.y -= PlayerSettings.WallSlideGravity * Time.deltaTime;
            Actor.CurrentVelocity.y = Mathf.Max( -PlayerSettings.MaxWallSlideVelocity, Actor.CurrentVelocity.y );

            // default move
            Actor.Move( Actor.CurrentVelocity * Time.deltaTime );

            Actor.CheckWallCollisions();

            if ( !Actor.CheckWallProximity( -Actor.Direction ) )
            {
                return new FallState( Actor );
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

            if ( Actor.CheckJump() )
            {
                return new WallJumpState( Actor );
            }

            return null;
        }
    }
}