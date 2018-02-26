﻿using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public abstract class FixedHorizontalMovementStateBase : FixedTimeStateBase
    {
        private float _direction;
        private float _lastNormalizedTime;
        private Vector2 _lastTangent;
        protected bool CurrentlyGrounded;

        protected FixedHorizontalMovementStateBase( Actor actor ) : base( actor )
        {
        }

        protected abstract float MovementLength { get; }

        protected abstract AnimationCurve MovementCurve { get; }

        public override void OnEnter()
        {
            base.OnEnter();
            _direction = Actor.Mobile.Direction;
            _lastNormalizedTime = 0;
        }

        protected void ApplyHorizontalMovement()
        {
            var mob = Actor.Mobile;

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