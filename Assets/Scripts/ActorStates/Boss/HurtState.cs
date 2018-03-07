using UnityEngine;

namespace ActorStates.Boss
{
    public class HurtState : BossFixedHorizontalMovementBase
    {
        private readonly float _direction;
        
        public HurtState( BossActor actor, float direction ) : base( actor )
        {
            _direction = direction;
        }

        protected override float TotalDuration
        {
            get { return Settings.HurtDuration; }
        }

        protected override float MovementLength
        {
            get { return _direction * Settings.HurtDriftLength; }
        }

        protected override AnimationCurve MovementCurve
        {
            get { return Settings.HurtDriftMovementCurve; }
        }

        public override IActorState Update()
        {
            ApplyHorizontalMovement();

            return base.Update();
        }

        protected override IActorState GetNextState()
        {
            if ( !Actor.GetComponent<ActorHealth>().IsAlive )
            {
                return new DeathState( Actor );
            }
            return base.GetNextState();
        }
    }
}