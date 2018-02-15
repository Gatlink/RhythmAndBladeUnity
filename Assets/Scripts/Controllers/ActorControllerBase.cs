using Gamelogic.Extensions;

namespace Controllers
{
    public abstract class ActorControllerBase : GLMonoBehaviour
    {
        public abstract void UpdateActorIntent( Actor actor );
    }
}