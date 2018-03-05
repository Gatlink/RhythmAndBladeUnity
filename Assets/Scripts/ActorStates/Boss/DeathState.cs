namespace ActorStates.Boss
{
    public class DeathState : BossActorStateBase
    {
        public DeathState( BossActor actor ) : base( actor )
        {
        }

        public override IActorState Update()
        {
            return null;
        }
    }
}