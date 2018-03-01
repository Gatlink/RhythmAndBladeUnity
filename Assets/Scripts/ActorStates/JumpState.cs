using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public class JumpState : PlayerFixedVerticalMovementState
    {
        // normalized time before wich ground is not checked yet
        private const float GroundCheckInhibitionTime = 1f;

        // normalized time after wich ceiling is not checked any more
        private const float CeilingCheckInhibitionTime = 0.8f;

        private float _jumpDirection;

        private JumpSetting _setting;

        private float JumpMovementSpeed
        {
            get { return _setting.HorizontalMovementSpeed; }
        }

        private float JumpMoveInertia
        {
            get { return _setting.HorizontalMovementInertia; }
        }

        private float AirControlTiming
        {
            get { return _setting.AirControlTiming; }
        }

        private float InitialMovementSpeed
        {
            get { return _setting.InitialMovementSpeed; }
        }

        protected override float TotalDuration
        {
            get { return _setting.Duration; }
        }

        protected override float MovementLength
        {
            get { return _setting.Height; }
        }

        protected override AnimationCurve MovementCurve
        {
            get { return _setting.HeightCurve; }
        }

        public JumpState( PlayerActor actor, JumpSetting setting ) : base( actor )
        {
            _setting = setting;
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
            base.OnEnter();
            _jumpDirection = Mobile.Direction;
            Actor.ConsumeJump();
            Mobile.SetHorizontalVelocity( GetHorizontalVelocity() );            
        }

        public override IActorState Update()
        {
            var mob = Mobile;
            var actor = Actor;
            
            var desiredVelocity = GetHorizontalVelocity();

            mob.UpdateDirection( desiredVelocity );

            // update current horizontal velocity accounting inertia
            mob.ChangeHorizontalVelocity( desiredVelocity, JumpMoveInertia );

            ApplyVerticalMovement();

            // default move
            mob.Move();

            var harmfull = actor.Health.CheckDamages();
            if ( harmfull != null )
            {
                return new HurtState( actor, harmfull );
            }

            if ( NormalizedTime < CeilingCheckInhibitionTime && mob.CheckCeiling() )
            {
                return new FallState( actor );
            }

            if ( NormalizedTime > GroundCheckInhibitionTime && mob.CheckGround() )
            {
                return new GroundedState( actor );
            }

            if ( actor.CheckDash() )
            {
                return new DashState( actor );
            }

            if ( actor.CheckAttack() )
            {
                return new AttackState( actor );
            }

            return base.Update();
        }
    }
}