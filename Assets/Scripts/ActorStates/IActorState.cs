namespace ActorStates
{
    public interface IActorState
    {
        void OnEnter();

        void OnExit();

        IActorState Update();
        
        string Name { get; }
    }
}