namespace ActorStates.Boss
{
    public abstract class BossFixedHorizontalMovementBase : FixedHorizontalMovementStateBase
    {
        protected readonly BossActor Actor;
        protected readonly Boss1Settings Settings;

        public BossFixedHorizontalMovementBase( BossActor actor ) : base( actor.Mobile )
        {
            Actor = actor;
            Settings = Boss1Settings.Instance;
        }

        protected override IActorState GetNextState()
        {
            return Mobile.CheckGround()
                ? (IActorState) new GroundedState( Actor )
                : new FallState( Actor );
        }
    }
}