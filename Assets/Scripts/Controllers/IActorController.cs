namespace Controllers
{
    public interface IActorController<in TActor> 
    {
        bool Enabled { get; }
        void UpdateActorIntent( TActor actor );
    }
}