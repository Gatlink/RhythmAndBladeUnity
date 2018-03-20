using UnityEngine;

namespace ActorStates.Player
{
    public class HurtState : FixedTimeStateBase
    {
        private readonly PlayerActor _actor;
        private readonly PlayerSettings _playerSettings;
        private readonly float _recoilDirection;
        private readonly float _recoilStrength;
        private readonly Vector2 _recoil;
        private readonly bool _teleportToCheckpointOnComplete;

        protected override float TotalDuration
        {
            get { return _playerSettings.HurtRecoilDuration; }
        }

        public HurtState( PlayerActor actor, IHarmfull harmfull, Vector2 recoil )
        {
            _actor = actor;
            _playerSettings = PlayerSettings.Instance;
            _recoil = recoil * harmfull.Recoil;
            _teleportToCheckpointOnComplete = harmfull.TeleportToLastCheckpoint;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _actor.Mobile.CurrentVelocity = _recoil;
        }

        public override IActorState Update()
        {
            var mob = _actor.Mobile;

            // apply gravity
            var verticalVelocity = mob.CurrentVelocity.y - _playerSettings.Gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max( verticalVelocity, -_playerSettings.MaxFallVelocity );
            mob.SetVerticalVelocity( verticalVelocity );

            // default move
            mob.Move();

            return base.Update();
        }

        protected override IActorState GetNextState()
        {
            if ( !_actor.GetComponent<ActorHealth>().IsAlive )
            {
                return new DeathState( _actor );
            }

            if ( _teleportToCheckpointOnComplete )
            {
                CheckpointManager.TeleportPlayerToLastCheckpoint();
                return new PauseState();
            }

            return _actor.Mobile.CheckGround()
                ? (IActorState) new GroundedState( _actor )
                : new FallState( _actor );
        }
    }
}