using UnityEngine;

namespace ActorStates
{
    public class DashJumpState : JumpState
    {
        protected override float JumpMovementSpeed
        {
            get { return PlayerSettings.DashJumpMovementSpeed; }
        }

        protected override float JumpMoveInertia
        {
            get { return PlayerSettings.DashJumpMoveInertia; }
        }

        protected override AnimationCurve JumpHeightCurve
        {
            get { return PlayerSettings.DashJumpHeightCurve; }
        }

        protected override float JumpDuration
        {
            get { return PlayerSettings.DashJumpDuration; }
        }

        protected override float JumpHeight
        {
            get { return PlayerSettings.DashJumpHeight; }
        }

        protected override float AirControlTiming
        {
            get { return PlayerSettings.DashJumpAirControlTiming; }
        }

        protected override float InitialMovementSpeed
        {
            get { return PlayerSettings.DashJumpInitialMovementSpeed; }
        }

        public DashJumpState( Actor actor ) : base( actor )
        {
        }
    }
}