using UnityEngine;

namespace ActorStates
{
    public class DashState : FixedHorizontalMovementStateBase
    {
        private bool _initiatedGrounded;

        public DashState( Actor actor ) : base( actor )
        {
        }

        protected override float TotalDuration
        {
            get { return PlayerSettings.DashDuration; }
        }

        protected override float MovementLength
        {
            get { return PlayerSettings.DashLength; }
        }

        protected override AnimationCurve MovementCurve
        {
            get { return PlayerSettings.DashPositionCurve; }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Actor.DashCount--;
            _initiatedGrounded = Actor.CheckGround();
        }

        public override IActorState Update()
        {
            ApplyHorizontalMovement();

            Vector2 wallNormal;
            if ( !CurrentlyGrounded && Actor.CheckWallProximity( Actor.Direction, out wallNormal ) )
            {
                return new WallSlideState( Actor, wallNormal );
            }

            if ( _initiatedGrounded
                 && NormalizedTime >= PlayerSettings.DashJumpTiming
                 && Actor.CheckJump() )
            {
                Actor.DashCount = 1;
                return new DashJumpState( Actor );
            }

            return ChangeStateOnFinish();
        }
    }
}