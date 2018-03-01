namespace ActorStates
{
    public abstract class ActorStateBase : IActorState
    {
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