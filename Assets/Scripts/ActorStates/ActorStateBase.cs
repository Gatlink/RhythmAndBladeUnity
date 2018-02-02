namespace ActorStates
{
    public abstract class ActorStateBase : IActorState
    {
        protected Actor Actor;
        protected PlayerSettings PlayerSettings;

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