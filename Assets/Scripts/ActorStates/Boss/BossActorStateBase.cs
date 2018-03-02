namespace ActorStates.Boss
{
    public abstract class BossActorStateBase : ActorStateBase
    {
        protected readonly BossActor Actor;
        protected readonly Boss1Settings Settings;
        
        protected BossActorStateBase( BossActor actor )
        {
            Actor = actor;
            Settings = Boss1Settings.Instance;            
        }
    }
}