namespace ActorStates
{
    public abstract class PlayerActorStateBase : ActorStateBase
    {
        protected readonly PlayerActor Actor;
        
        protected readonly PlayerSettings PlayerSettings;

        protected PlayerActorStateBase( PlayerActor actor )
        {
            Actor = actor;
            PlayerSettings = PlayerSettings.Instance;
        }        
    }
}