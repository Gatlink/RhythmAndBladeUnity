using UnityEngine;

namespace ActorStates.Player
{
    public class HurtState : FixedTimeStateBase
    {
        private readonly PlayerActor _actor;
        private readonly PlayerSettings _playerSettings;
        private readonly float _recoilDirection;
        private readonly float _recoilStrength;

        protected override float TotalDuration
        {
            get { return _playerSettings.HurtRecoilDuration; }
        }

        private float RecoilLength
        {
            get { return _playerSettings.HurtRecoilDistance * _recoilStrength; }
        }

        public HurtState( PlayerActor actor, GameObject source, float recoilStrength )
        {
            _actor = actor;
            _playerSettings = PlayerSettings.Instance;
            _recoilDirection = Mathf.Sign( _actor.transform.position.x - source.transform.position.x );
            _recoilStrength = recoilStrength;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            var angle = Mathf.Atan( TotalDuration * TotalDuration * _playerSettings.Gravity / 2 / RecoilLength );
            var velocity = TotalDuration * _playerSettings.Gravity / 2 / Mathf.Sin( angle );
            _actor.Mobile.CurrentVelocity =
                new Vector2( _recoilDirection * Mathf.Cos( angle ), Mathf.Sin( angle ) ) * velocity;
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

            return _actor.Mobile.CheckGround()
                ? (IActorState) new GroundedState( _actor )
                : new FallState( _actor );
        }
    }
}