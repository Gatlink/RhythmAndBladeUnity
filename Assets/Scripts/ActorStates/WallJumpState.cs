namespace ActorStates
{
    public class WallJumpState : JumpState
    {
        private float _jumpDirection;

        protected float AirControlTiming
        {
            get { return PlayerSettings.JumpAirControlTiming; }
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

        public WallJumpState( Actor actor ) : base( actor )
        {
        }
    }
}