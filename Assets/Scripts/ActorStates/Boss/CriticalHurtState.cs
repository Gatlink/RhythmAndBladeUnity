namespace ActorStates.Boss
{
    public class CriticalHurtState : HurtState
    {
        public CriticalHurtState( BossActor actor, float direction ) : base( actor, direction )
        {
        }

        protected override float TotalDuration
        {
            get { return Settings.CriticalHurtDuration; }
        }

        protected override float MovementLength
        {
            get { return Settings.CriticalHurtDriftLength; }
        }
    }
}