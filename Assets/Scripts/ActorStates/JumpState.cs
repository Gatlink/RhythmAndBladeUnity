using UnityEngine;

namespace ActorStates
{
    public class JumpState : ActorStateBase
    {
        private float _jumpTimeRemaining;
        private float _jumpStartPositionY;

        public JumpState( Actor actor ) : base( actor )
        {
        }

        public override void OnEnter()
        {
            _jumpTimeRemaining = PlayerSettings.JumpDuration;
            _jumpStartPositionY = Actor.transform.position.y;
        }

        public override IActorState Update()
        {
            var desiredVelocity = Actor.DesiredMovement * PlayerSettings.JumpMovementSpeed;

            Actor.UpdateDirection( desiredVelocity );

            // update current horizontal velocity accounting inertia
            Actor.CurrentVelocity.x = Mathf.SmoothDamp( Actor.CurrentVelocity.x, desiredVelocity,
                ref Actor.CurrentAcceleration.x, PlayerSettings.JumpMoveInertia );

            // apply jump vertical velocity curve
            var targetPositionY = _jumpStartPositionY +
                                  PlayerSettings.JumpHeightCurve.Evaluate(
                                      1 - _jumpTimeRemaining / PlayerSettings.JumpDuration ) *
                                  PlayerSettings.JumpHeight;
            Actor.CurrentVelocity.y = (targetPositionY - Actor.transform.position.y) / Time.deltaTime;

            // default move
            Actor.Move( Actor.CurrentVelocity * Time.deltaTime );

            Actor.CheckWallCollisions();
            
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