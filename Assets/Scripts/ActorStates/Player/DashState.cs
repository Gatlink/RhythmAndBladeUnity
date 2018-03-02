using UnityEngine;

namespace ActorStates.Player
{
    public class DashState : PlayerFixedHorizontalMovementState
    {
        private bool _initiatedGrounded;
        private readonly PlayerSettings _playerSettings;

        public DashState( PlayerActor actor ) : base( actor )
        {
            _playerSettings = PlayerSettings.Instance;            
        }

        protected override float TotalDuration
        {
            get { return _playerSettings.DashDuration; }
        }

        protected override float MovementLength
        {
            get { return _playerSettings.DashLength; }
        }

        protected override AnimationCurve MovementCurve
        {
            get { return _playerSettings.DashPositionCurve; }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Actor.ConsumeDash();
            _initiatedGrounded = Mobile.CheckGround();
        }

        public override IActorState Update()
        {
            ApplyHorizontalMovement();

            Vector2 wallNormal;
            if ( !CurrentlyGrounded && Mobile.CheckWallProximity( Mobile.Direction, out wallNormal ) )
            {
                return new WallSlideState( Actor, wallNormal );
            }

            if ( _initiatedGrounded
                 && NormalizedTime >= _playerSettings.DashJumpTiming
                 && Actor.CheckJump() )
            {
                Actor.ResetDash();
                return new JumpState( Actor, _playerSettings.DashJump );
            }

            return base.Update();
        }
    }
}