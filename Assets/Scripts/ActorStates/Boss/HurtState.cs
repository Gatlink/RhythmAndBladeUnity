using UnityEngine;

namespace ActorStates.Boss
{
    public class HurtState : BossFixedHorizontalMovementBase
    {
        private readonly float _hurtDirection;

        public HurtState( BossActor actor, float direction ) : base( actor )
        {
            _hurtDirection = direction;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Direction = _hurtDirection;
            Mobile.UpdateDirection( -_hurtDirection );
        }

        protected override float TotalDuration
        {
            get { return Settings.HurtDuration; }
        }

        protected override float MovementLength
        {
            get { return Settings.HurtDriftLength; }
        }

        protected override Easing MovementTrajectory
        {
            get { return Settings.HurtDriftTrajectory; }
        }

        public override IActorState Update()
        {
            ApplyHorizontalMovement();

            // apply gravity
            var verticalVelocity = Mobile.CurrentVelocity.y - Settings.Gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max( verticalVelocity, -Settings.MaxFallVelocity );
            Mobile.SetVerticalVelocity( verticalVelocity );

            // default move
            Mobile.Move();

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