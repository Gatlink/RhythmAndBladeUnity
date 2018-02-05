using UnityEngine;

namespace ActorStates
{
    public class JumpState : ActorStateBase
    {
        private float _jumpTimeRemaining;
        private float _jumpStartPositionY;
        private float _jumpDirection;            

        protected virtual float JumpDuration
        {
            get { return PlayerSettings.JumpDuration; }
        }

        protected virtual float JumpMovementSpeed
        {
            get { return PlayerSettings.JumpMovementSpeed; }
        }

        protected virtual float JumpMoveInertia
        {
            get { return PlayerSettings.JumpMoveInertia; }
        }

        protected virtual AnimationCurve JumpHeightCurve
        {
            get { return PlayerSettings.JumpHeightCurve; }
        }

        protected virtual float JumpHeight
        {
            get { return PlayerSettings.JumpHeight; }
        }

        protected virtual float AirControlTiming
        {
            get { return PlayerSettings.JumpAirControlTiming; }
        }

        protected virtual float InitialMovementSpeed
        {
            get { return PlayerSettings.JumpInitialMovementSpeed; }
        }

        private float NormalizedTime
        {
            get { return 1 - _jumpTimeRemaining / JumpDuration; }
        }

        public JumpState( Actor actor ) : base( actor )
        {
        }

        private float GetHorizontalVelocity()
        {
            if ( NormalizedTime < AirControlTiming )
            {
                return InitialMovementSpeed * _jumpDirection;
            }
            return Actor.DesiredMovement * JumpMovementSpeed;
        }

        public override void OnEnter()
        {
            _jumpTimeRemaining = JumpDuration;
            _jumpStartPositionY = Actor.transform.position.y;            
            _jumpDirection = Actor.Direction;
            Actor.CurrentVelocity.x = GetHorizontalVelocity();
        }

        public override IActorState Update()
        {
            var desiredVelocity = GetHorizontalVelocity();

            Actor.UpdateDirection( desiredVelocity );

            // update current horizontal velocity accounting inertia
            Actor.CurrentVelocity.x = Mathf.SmoothDamp( Actor.CurrentVelocity.x, desiredVelocity,
                ref Actor.CurrentAcceleration.x, JumpMoveInertia );

            // apply jump vertical velocity curve
            var targetPositionY = _jumpStartPositionY +
                                  JumpHeightCurve.Evaluate( NormalizedTime ) * JumpHeight;
            Actor.CurrentVelocity.y = ( targetPositionY - Actor.transform.position.y ) / Time.deltaTime;

            // default move
            Actor.Move( Actor.CurrentVelocity * Time.deltaTime );

            Actor.CheckWallCollisions();

            if ( Actor.CheckDash() )
            {
                return new DashState( Actor );
            }

            if ( Actor.CheckAttack() )
            {
                return new AttackState( Actor );
            }

            _jumpTimeRemaining -= Time.deltaTime;
            if ( _jumpTimeRemaining <= 0 )
            {
                return new FallState( Actor );
            }

            return null;
        }
    }
}