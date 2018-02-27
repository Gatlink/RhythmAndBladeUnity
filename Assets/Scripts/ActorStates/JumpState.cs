﻿using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public class JumpState : PlayerActorStateBase
    {
        // normalized time before wich ground is not checked yet
        private const float GroundCheckInhibitionTime = 1f;

        // normalized time after wich ceiling is not checked any more
        private const float CeilingCheckInhibitionTime = 0.8f;

        private float _jumpTimeRemaining;
        private float _jumpStartPositionY;
        private float _jumpDirection;

        private JumpSetting _setting;

        private float JumpDuration
        {
            get { return _setting.Duration; }
        }

        private float JumpMovementSpeed
        {
            get { return _setting.HorizontalMovementSpeed; }
        }

        private float JumpMoveInertia
        {
            get { return _setting.HorizontalMovementInertia; }
        }

        private AnimationCurve JumpHeightCurve
        {
            get { return _setting.HeightCurve; }
        }

        private float JumpHeight
        {
            get { return _setting.Height; }
        }

        private float AirControlTiming
        {
            get { return _setting.AirControlTiming; }
        }

        private float InitialMovementSpeed
        {
            get { return _setting.InitialMovementSpeed; }
        }

        private float NormalizedTime
        {
            get { return 1 - _jumpTimeRemaining / JumpDuration; }
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
            _jumpTimeRemaining = JumpDuration;
            _jumpStartPositionY = Actor.transform.position.y;
            _jumpDirection = Actor.Mobile.Direction;
            Actor.ConsumeJump();
            Actor.Mobile.CurrentVelocity = Actor.Mobile.CurrentVelocity.WithX( GetHorizontalVelocity() );
        }

        public override IActorState<PlayerActor> Update()
        {
            var mob = Actor.Mobile;
            var desiredVelocity = GetHorizontalVelocity();

            mob.UpdateDirection( desiredVelocity );

            // update current horizontal velocity accounting inertia
            mob.ChangeHorizontalVelocity( desiredVelocity, JumpMoveInertia );
            
            // apply jump vertical velocity curve
            var targetPositionY = _jumpStartPositionY + JumpHeightCurve.Evaluate( NormalizedTime ) * JumpHeight;
            mob.SetVerticalVelocity( ( targetPositionY - Actor.transform.position.y ) / Time.deltaTime );
            
            // default move
            mob.Move();

            var harmfull = Actor.Health.CheckDamages();
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