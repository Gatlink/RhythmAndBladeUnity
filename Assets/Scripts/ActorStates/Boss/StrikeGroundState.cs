namespace ActorStates.Boss
{
    public class StrikeGroundState : BossFixedTimeStateBase
    {
        public StrikeGroundState( BossActor actor ) : base( actor )
        {
        }

        protected override float TotalDuration
        {
            get { return Settings.StrikeGroundDuration; }
        }

        protected override IActorState GetNextState()
        {
            return Actor.Mobile.CheckGround()
                ? (IActorState) new GroundedState( Actor )
                : new FallState( Actor );
        }
    }
}