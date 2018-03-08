using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public abstract class FixedHorizontalMovementStateBase : FixedTimeStateBase
    {
        protected readonly Mobile Mobile;
        protected float Direction;
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
            Direction = Mobile.Direction;
            _lastNormalizedTime = 0;
        }

        protected void ApplyHorizontalMovement()
        {
            var mob = Mobile;

            // apply tangencial velocity curve
            var deltaU = Direction * MovementLength *
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
        }
    }
}