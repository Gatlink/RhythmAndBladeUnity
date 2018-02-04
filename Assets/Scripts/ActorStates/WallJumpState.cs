namespace ActorStates
{
    public class WallJumpState : JumpState
    {
        private float _jumpDirection;

        private float AirControlTiming
        {
            get { return PlayerSettings.WallJumpAirControlTiming; }
        }

        public WallJumpState( Actor actor ) : base( actor )
        {
        }

        protected override float GetVelocity()
        {
            if ( NormalizedTime < AirControlTiming )
            {
                return JumpMovementSpeed * _jumpDirection;
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