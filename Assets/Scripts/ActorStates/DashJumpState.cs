using UnityEngine;

namespace ActorStates
{
    public class DashJumpState : JumpState
    {
        private float _jumpDirection;

        protected override float JumpDuration
        {
            get { return PlayerSettings.DashJumpDuration; }
        }

        protected override float JumpHeight
        {
            get { return PlayerSettings.DashJumpHeight; }
        }

        private float AirControlTiming
        {
            get { return PlayerSettings.DashJumpAirControlTiming; }
        }

        public DashJumpState( Actor actor ) : base( actor )
        {
        }

        protected override float GetVelocity()
        {
            if ( NormalizedTime < AirControlTiming )
            {
                return PlayerSettings.DashJumpInitialMovementSpeed * _jumpDirection;
            }
            return base.GetVelocity();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _jumpDirection = Actor.Direction;
            Actor.CurrentVelocity.x = GetVelocity();
        }
    }
}