using UnityEngine;
using UnityEngine.AI;

namespace ActorStates
{
    public class DashState : ActorStateBase
    {
        private float _dashTimeRemaining;
        private float _dashDirection;
        private float _dashStartPositionX;
        private bool _groundedDash;

        public DashState( Actor actor ) : base( actor )
        {
        }

        public override void OnEnter()
        {
            Actor.DashCount--;
            _dashTimeRemaining = PlayerSettings.DashDuration;
            _dashDirection = Actor.Direction;
            _dashStartPositionX = Actor.transform.position.x;
            _groundedDash = Actor.CheckGround( snap: false );
        }

        public override IActorState Update()
        {
            // apply dash horizontal velocity curve
            var targetPositionX = _dashStartPositionX + _dashDirection *
                                  PlayerSettings.DashPositionCurve.Evaluate( NormalizedTime ) *
                                  PlayerSettings.DashLength;

            // compute a velocity that realizes desired position curve
            Actor.CurrentVelocity.x = ( targetPositionX - Actor.transform.position.x ) / Time.deltaTime;

            Actor.CurrentVelocity.y = 0;

            // default move
            Actor.Move( Actor.CurrentVelocity * Time.deltaTime );

            Actor.CheckWallCollisions();

            Vector2 normal;
            if ( Actor.CheckWallProximity( Actor.Direction, out normal ) )
            {
                return new WallSlideState( Actor, normal );
            }

            if ( NormalizedTime >= PlayerSettings.DashJumpTiming
                 && Actor.CheckJump()
                 && _groundedDash )
            {
                Actor.DashCount = 1;
                return new DashJumpState( Actor );
            }

            _dashTimeRemaining -= Time.deltaTime;
            if ( _dashTimeRemaining <= 0 )
            {
                if ( Actor.CheckGround() )
                {
                    return new GroundedState( Actor );
                }
                else
                {
                    return new FallState( Actor );
                }
            }

            return null;
        }

        private float NormalizedTime
        {
            get { return 1 - _dashTimeRemaining / PlayerSettings.DashDuration; }
        }
    }
}