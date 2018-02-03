using Gamelogic.Extensions;

public abstract class ActorControllerBase : GLMonoBehaviour
{
    public abstract void UpdateActorIntent( Actor actor );
}