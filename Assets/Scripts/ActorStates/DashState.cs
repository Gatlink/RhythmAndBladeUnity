using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public class DashState : ActorStateBase
    {
        private float _timeRemaining;
        private float _direction;
        private bool _initiatedGrounded;
        private float _lastNormalizedTime;
        private Vector2 _lastTangent;

        protected virtual float TotalDuration
        {
            get { return PlayerSettings.DashDuration; }
        }

        public DashState( Actor actor ) : base( actor )
        {
        }

        public override void OnEnter()
        {
            Actor.DashCount--;
            _timeRemaining = TotalDuration;
            _direction = Actor.Direction;
            _initiatedGrounded = Actor.CheckGround();
            _lastNormalizedTime = 0;
        }

        public override IActorState Update()
        {
            // apply dash tangencial velocity curve
            var deltaU = _direction * MovementLength *
                         ( MovementCurve.Evaluate( NormalizedTime ) - MovementCurve.Evaluate( _lastNormalizedTime ) );

            _lastNormalizedTime = NormalizedTime;

            Vector2 normal;
            var currentlyGrounded = Actor.CheckGround( out normal );

            Vector2 tangent;
            if ( currentlyGrounded )
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

            if ( !currentlyGrounded && Actor.CheckWallProximity( Actor.Direction, out normal ) )
            {
                return new WallSlideState( Actor, normal );
            }

            if ( NormalizedTime >= PlayerSettings.DashJumpTiming
                 && Actor.CheckJump()
                 && _initiatedGrounded )
            {
                Actor.DashCount = 1;
                return new DashJumpState( Actor );
            }

            _timeRemaining -= Time.deltaTime;
            if ( _timeRemaining <= 0 )
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

        protected virtual AnimationCurve MovementCurve
        {
            get { return PlayerSettings.DashPositionCurve; }
        }

        protected virtual float MovementLength
        {
            get { return PlayerSettings.DashLength; }
        }

        private float NormalizedTime
        {
            get { return 1 - _timeRemaining / PlayerSettings.DashDuration; }
        }
    }
}