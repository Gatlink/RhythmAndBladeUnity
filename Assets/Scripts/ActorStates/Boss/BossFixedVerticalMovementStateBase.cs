namespace ActorStates.Boss
{
    public abstract class BossFixedVerticalMovementStateBase : FixedVerticalMovementStateBase
    {
        protected readonly BossActor Actor;
        protected readonly Boss1Settings Settings;

        public BossFixedVerticalMovementStateBase( BossActor actor ) : base( actor.Mobile )
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