namespace ActorStates
{
    public abstract class ActorStateBase : IActorState
    {
        protected readonly Actor Actor;
        protected readonly PlayerSettings PlayerSettings;

        protected ActorStateBase( Actor actor )
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

        public abstract IActorState Update();

        public string Name
        {
            get { return GetType().Name; }
        }
    }
}