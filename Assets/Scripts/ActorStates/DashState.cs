using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public class DashState : ActorStateBase
    {
        private float _dashTimeRemaining;
        private float _dashDirection;
        private bool _groundedDash;
        private float _lastNormalizedTime;
        private Vector2 _lastTangent;

        public DashState( Actor actor ) : base( actor )
        {
        }

        public override void OnEnter()
        {
            Actor.DashCount--;
            _dashTimeRemaining = PlayerSettings.DashDuration;
            _dashDirection = Actor.Direction;
            _groundedDash = Actor.CheckGround();
            _lastNormalizedTime = 0;
        }

        public override IActorState Update()
        {
            // apply dash tangencial velocity curve
            var deltaU = _dashDirection * PlayerSettings.DashLength *
                         ( PlayerSettings.DashPositionCurve.Evaluate( NormalizedTime ) -
                           PlayerSettings.DashPositionCurve.Evaluate( _lastNormalizedTime ) );

            _lastNormalizedTime = NormalizedTime;

            Vector2 tangent;
            Vector2 normal;
            if ( _groundedDash && Actor.CheckGround( out normal ) )
            {
                tangent = normal.Rotate270();
            }
            else
            {
                tangent = Vector2.right;
            }

            Actor.CurrentVelocity = tangent * deltaU / Time.deltaTime;
            
            // default move
            Actor.Move( Actor.CurrentVelocity * Time.deltaTime );

            Actor.CheckWallCollisions();

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