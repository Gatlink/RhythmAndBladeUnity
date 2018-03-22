using Gamelogic.Extensions;

namespace Controllers
{
    public abstract class BossControllerBase : GLMonoBehaviour, IActorController<BossActor>
    {
        public string Name;
        public OptionalInt HealthEndCondition;

        private bool _actorCriticalHurtFlagSet;

        public bool Enabled
        {
            get { return enabled; }
            protected set { enabled = value; }
        }

        public virtual void UpdateActorIntent( BossActor actor )
        {
            if ( HealthEndCondition.UseValue )
            {
                var currentHealth = actor.GetComponent<ActorHealth>().CurrentHitCount;
                if ( !_actorCriticalHurtFlagSet && currentHealth <= HealthEndCondition.Value + 1 )
                {
                    _actorCriticalHurtFlagSet = true;
                    actor.SetNextHurtIsCritical();
                }
                if ( currentHealth <= HealthEndCondition.Value )
                {
                    enabled = false;
                }
            }
        }
    }
}