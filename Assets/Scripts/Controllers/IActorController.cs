namespace Controllers
{
    public interface IActorController<in TActor> 
    {
        void UpdateActorIntent( TActor actor );
    }
}