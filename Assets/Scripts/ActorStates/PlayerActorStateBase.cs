namespace ActorStates
{
    public abstract class PlayerActorStateBase : IActorState<PlayerActor>
    {
        protected readonly PlayerActor Actor;
        
        protected readonly PlayerSettings PlayerSettings;

        protected PlayerActorStateBase( PlayerActor actor )
        {
            Actor = actor;
            PlayerSettings = PlayerSettings.Instance;
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        public abstract IActorState<PlayerActor> Update();

        public string Name
        {
            get { return GetType().Name; }
        }
    }
}