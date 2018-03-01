using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public abstract class FixedHorizontalMovementStateBase : FixedTimeStateBase
    {
        protected readonly Mobile Mobile;
        private float _direction;
        private float _lastNormalizedTime;
        private Vector2 _lastTangent;
        protected bool CurrentlyGrounded;

        protected FixedHorizontalMovementStateBase( Mobile mobile )
        {
            Mobile = mobile;
        }

        protected abstract float MovementLength { get; }

        protected abstract AnimationCurve MovementCurve { get; }

        public override void OnEnter()
        {
            base.OnEnter();
            _direction = Mobile.Direction;
            _lastNormalizedTime = 0;
        }

        protected void ApplyHorizontalMovement()
        {
            var mob = Mobile;

            // apply tangencial velocity curve
            var deltaU = _direction * MovementLength *
                         ( MovementCurve.Evaluate( NormalizedTime ) - MovementCurve.Evaluate( _lastNormalizedTime ) );

            _lastNormalizedTime = NormalizedTime;

            Vector2 groundNormal;
            CurrentlyGrounded = mob.CheckGround( out groundNormal );

            Vector2 tangent;
            if ( CurrentlyGrounded )
            {
                tangent = groundNormal.Rotate270();
            }
            else
            {
                tangent = Vector2.right;
            }

            mob.CurrentVelocity = tangent * deltaU / Time.deltaTime;

            // default move
            mob.Move();
        }
    }
}