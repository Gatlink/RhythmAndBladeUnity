using UnityEngine;

namespace ActorStates
{
    public class WallJumpState : JumpState
    {
        protected override float JumpDuration
        {
            get { return PlayerSettings.WallJumpDuration; }
        }

        protected override float JumpMovementSpeed
        {
            get { return PlayerSettings.WallJumpMovementSpeed; }
        }

        protected override float JumpMoveInertia
        {
            get { return PlayerSettings.WallJumpMoveInertia; }
        }

        protected override AnimationCurve JumpHeightCurve
        {
            get { return PlayerSettings.WallJumpHeightCurve; }
        }

        protected override float JumpHeight
        {
            get { return PlayerSettings.WallJumpHeight; }
        }

        protected override float AirControlTiming
        {
            get { return PlayerSettings.WallJumpAirControlTiming; }
        }

        protected override float InitialMovementSpeed
        {
            get { return PlayerSettings.WallJumpInitialMovementSpeed; }
        }

        public WallJumpState( Actor actor ) : base( actor )
        {
        }
    }
}