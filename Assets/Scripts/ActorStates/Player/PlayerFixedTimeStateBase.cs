namespace ActorStates.Player
{
    public abstract class PlayerFixedTimeStateBase : FixedTimeStateBase
    {
        protected readonly PlayerActor Actor;
        
        protected readonly PlayerSettings PlayerSettings;

        protected PlayerFixedTimeStateBase( PlayerActor actor )
        {
            Actor = actor;
            PlayerSettings = PlayerSettings.Instance;
        }
        protected override IActorState GetNextState()
        {
            if ( !Actor.GetComponent<ActorHealth>().IsAlive )
            {
                return new DeathState( Actor );
            }

            return Actor.Mobile.CheckGround()
                ? (IActorState) new GroundedState( Actor )
                : new FallState( Actor );
        }
    }
}