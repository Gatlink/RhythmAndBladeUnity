namespace ActorStates.Boss
{
    public class ChargeAttackState : BossFixedHorizontalMovementBase
    {
        private readonly float _distance;

        public ChargeAttackState( BossActor actor, float distance ) : base( actor )
        {
            _distance = distance;
        }

        protected override float TotalDuration
        {
            get { return Settings.ChargeDuration; }
        }

        protected override float MovementLength
        {
            get { return _distance; }
        }

        protected override Easing MovementTrajectory
        {
            get { return Settings.ChargeTrajectory; }
        }

        public override IActorState Update()
        {
            ApplyHorizontalMovement();

            Mobile.Move();

            if ( Mobile.CheckWallProximity( Mobile.Direction, snap: false ) )
            {
                Mobile.CancelHorizontalMovement();
                TerminateState();
            }

            return base.Update();
        }
    }
}