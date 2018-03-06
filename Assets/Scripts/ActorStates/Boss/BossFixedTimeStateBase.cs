namespace ActorStates.Boss
{
    public abstract class BossFixedTimeStateBase : FixedTimeStateBase
    {
        protected readonly BossActor Actor;
        protected readonly Boss1Settings Settings;

        public BossFixedTimeStateBase( BossActor actor )
        {
            Actor = actor;
            Settings = Boss1Settings.Instance;
        }
    }
}