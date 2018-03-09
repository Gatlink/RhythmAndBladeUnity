using UnityEngine;

namespace ActorStates
{
    public abstract class FixedVerticalMovementStateBase : FixedTimeStateBase
    {
        protected readonly Mobile Mobile;

        private float _jumpStartPositionY;

        protected abstract Easing MovementTrajectory { get; }

        protected abstract float MovementLength { get; }

        public FixedVerticalMovementStateBase( Mobile mobile )
        {
            Mobile = mobile;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _jumpStartPositionY = Mobile.transform.position.y;
        }

        protected void ApplyVerticalMovement()
        {
            // apply vertical velocity curve
            var targetPositionY = _jumpStartPositionY + MovementTrajectory.Eval( NormalizedTime ) * MovementLength;
            Mobile.SetVerticalVelocity( ( targetPositionY - Mobile.transform.position.y ) / Time.deltaTime );
        }
    }
}