using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public abstract class FixedHorizontalMovementStateBase : ActorStateBase
    {
        private float _timeRemaining;
        private float _direction;
        private float _lastNormalizedTime;
        private Vector2 _lastTangent;
        protected bool CurrentlyGrounded;

        protected FixedHorizontalMovementStateBase( Actor actor ) : base( actor )
        {
        }

        protected abstract float TotalDuration { get; }
        
        protected abstract float MovementLength { get; }
        
        protected abstract AnimationCurve MovementCurve { get; }

        protected float ElapsedTime
        {
            get { return TotalDuration - _timeRemaining; }
        }
        
        protected float NormalizedTime
        {
            get { return ElapsedTime / TotalDuration; }
        }

        public override void OnEnter()
        {
            _timeRemaining = TotalDuration;
            _direction = Actor.Direction;
            _lastNormalizedTime = 0;
        }

        protected IActorState ChangeStateOnFinish()
        {
            _timeRemaining -= Time.deltaTime;
            if ( _timeRemaining <= 0 )
            {
                if ( Actor.CheckGround() )
                {
                    return new GroundedState( Actor );
                }
                else
                {
                    return new FallState( Actor );
                }
            }

            return null;
        }

        protected void ApplyHorizontalMovement()
        {
            // apply tangencial velocity curve
            var deltaU = _direction * MovementLength *
                         ( MovementCurve.Evaluate( NormalizedTime ) - MovementCurve.Evaluate( _lastNormalizedTime ) );

            _lastNormalizedTime = NormalizedTime;

            Vector2 groundNormal;
            CurrentlyGrounded = Actor.CheckGround( out groundNormal );

            Vector2 tangent;
            if ( CurrentlyGrounded )
            {
                tangent = groundNormal.Rotate270();
            }
            else
            {
                tangent = Vector2.right;
            }

            Actor.CurrentVelocity = tangent * deltaU / Time.deltaTime;

            // default move
            Actor.Move( Actor.CurrentVelocity * Time.deltaTime );

            Actor.CheckWallCollisions();
        }
    }
}