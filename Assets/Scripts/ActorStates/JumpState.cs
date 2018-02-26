using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public class JumpState : ActorStateBase
    {
        // normalized time before wich ground is not checked yet
        private const float GroundCheckInhibitionTime = 1f;

        // normalized time after wich ceiling is not checked any more
        private const float CeilingCheckInhibitionTime = 0.8f;

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
            _jumpDirection = Actor.Mobile.Direction;
            Actor.ConsumeJump();
            Actor.Mobile.CurrentVelocity = Actor.Mobile.CurrentVelocity.WithX( GetHorizontalVelocity() );
        }

        public override IActorState Update()
        {
            var mob = Actor.Mobile;
            var desiredVelocity = GetHorizontalVelocity();

            mob.UpdateDirection( desiredVelocity );

            var velocity = mob.CurrentVelocity;
            var acceleration = mob.CurrentAcceleration;

            // update current horizontal velocity accounting inertia
            velocity.x = Mathf.SmoothDamp( velocity.x, desiredVelocity, ref acceleration.x, JumpMoveInertia );

            // apply jump vertical velocity curve
            var targetPositionY = _jumpStartPositionY + JumpHeightCurve.Evaluate( NormalizedTime ) * JumpHeight;
            velocity.y = ( targetPositionY - Actor.transform.position.y ) / Time.deltaTime;

            mob.CurrentVelocity = velocity;
            mob.CurrentAcceleration = acceleration;

            // default move
            mob.Move();

            var harmfull = Actor.CheckDamages();
            if ( harmfull != null )
            {
                return new HurtState( Actor, harmfull );
            }

            if ( NormalizedTime < CeilingCheckInhibitionTime && mob.CheckCeiling() )
            {
                return new FallState( Actor );
            }

            if ( NormalizedTime > GroundCheckInhibitionTime && mob.CheckGround() )
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

            _jumpTimeRemaining -= Time.deltaTime;
            if ( _jumpTimeRemaining <= 0 )
            {
                return new FallState( Actor );
            }

            return null;
        }
    }
}