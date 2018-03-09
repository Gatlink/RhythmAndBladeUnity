using UnityEngine;

namespace ActorStates.Boss
{
    public class JumpAttackState : BossFixedVerticalMovementStateBase
    {
        private readonly float _jumpHeight;
        private readonly float _horizontalDistance;
        private float _lastNormalizedTime;
        private float _direction;

        public JumpAttackState( BossActor actor, float height, float distance ) : base( actor )
        {
            _jumpHeight = height;
            _horizontalDistance = distance;
        }

        protected override float TotalDuration
        {
            get { return Settings.JumpAttackDuration; }
        }

        protected override Easing MovementTrajectory
        {
            get { return Settings.JumpAttackHeightTrajectory; }
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
            ApplyVerticalMovement();

            ApplyHorizontalMovement();

            // default move
            Mobile.Move();

            return base.Update();
        }

        protected override IActorState GetNextState()
        {
            return new DiveState( Actor );
        }

        private void ApplyHorizontalMovement()
        {
            var mob = Mobile;
            var curve = Settings.JumpAttackMovementTrajectory;

            // apply tangencial velocity curve
            var delta = _horizontalDistance *
                         ( curve.Eval( NormalizedTime ) - curve.Eval( _lastNormalizedTime ) );

            _lastNormalizedTime = NormalizedTime;

            mob.SetHorizontalVelocity( _direction * delta / Time.deltaTime );
        }
    }
}