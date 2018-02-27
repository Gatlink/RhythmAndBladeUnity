namespace ActorStates
{
    public interface IActorState<out TActor>
    {
        void OnEnter();

        void OnExit();

        IActorState<TActor> Update();
        
        string Name { get; }
    }
}