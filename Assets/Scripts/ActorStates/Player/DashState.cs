using UnityEngine;

namespace ActorStates.Player
{
    public class DashState : PlayerFixedHorizontalMovementState
    {
        private bool _initiatedGrounded;
        private readonly PlayerSettings _playerSettings;
        private readonly bool _useCurrentDirection;

        public DashState( PlayerActor actor, bool useCurrentDirection = false ) : base( actor )
        {
            _playerSettings = PlayerSettings.Instance;
            _useCurrentDirection = useCurrentDirection;
        }

        protected override float TotalDuration
        {
            get { return _playerSettings.DashDuration; }
        }

        protected override float MovementLength
        {
            get { return _playerSettings.DashLength; }
        }

        protected override Easing MovementTrajectory
        {
            get { return _playerSettings.DashTrajectory; }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if ( !_useCurrentDirection && Actor.DesiredMovement != 0 )
            {
                Direction = Actor.DesiredMovement;
                Mobile.UpdateDirection( Direction );
            }

            Actor.ConsumeDash();
            _initiatedGrounded = Mobile.CheckGround();
        }

        public override void OnExit()
        {
            base.OnExit();
            // prevent allowing an air jump using an attack when falling after a dash from a platform border
            Actor.ConsumeJump();
        }

        public override IActorState Update()
        {
            ApplyHorizontalMovement();

            Mobile.Move( isDash: true );

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

            if ( NormalizedTime >= _playerSettings.DashAttackTiming && Actor.CheckAttack() )
            {
                return new AttackState( Actor );
            }

            return base.Update();
        }
    }
}