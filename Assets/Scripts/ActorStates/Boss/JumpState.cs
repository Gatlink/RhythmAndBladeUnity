using UnityEngine;

namespace ActorStates.Boss
{
    public class JumpState : BossFixedVerticalMovementStateBase
    {
        private readonly float _jumpHeight;
        private readonly float _horizontalDistance;
        private float _lastNormalizedTime;
        private float _direction;

        public JumpState( BossActor actor, float height, float distance ) : base( actor )
        {
            _jumpHeight = height;
            _horizontalDistance = distance;
        }

        protected override float TotalDuration
        {
            get { return Mathf.Max(_horizontalDistance / Settings.JumpHorizontalSpeed, Settings.JumpMinDuration); }
        }

        protected override Easing MovementTrajectory
        {
            get { return Settings.JumpHeightTrajectory; }
        }

        protected virtual Easing HorizontalMovementTrajectory
        {
            get { return Settings.JumpMovementTrajectory; }
        }

        protected override float MovementLength
        {
            get { return _jumpHeight; }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _direction = Mobile.Direction;
            _lastNormalizedTime = 0;
        }

        public override IActorState Update()
        {
            var hurtState = Actor.GetHurtState();
            if ( hurtState != null )
            {
                return hurtState;
            }

            ApplyVerticalMovement();

            ApplyHorizontalMovement();

            // default move
            Mobile.Move();

            return base.Update();
        }

        private void ApplyHorizontalMovement()
        {
            var mob = Mobile;
            var curve = HorizontalMovementTrajectory;

            // apply tangencial velocity curve
            var delta = _horizontalDistance *
                        ( curve.Eval( NormalizedTime ) - curve.Eval( _lastNormalizedTime ) );

            _lastNormalizedTime = NormalizedTime;

            mob.SetHorizontalVelocity( _direction * delta / Time.deltaTime );
        }
    }
}