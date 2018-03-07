using UnityEngine;

namespace ActorStates.Player
{
    public class HurtState : FixedTimeStateBase
    {
        private readonly PlayerActor _actor;
        private readonly PlayerSettings _playerSettings;
        private readonly float _recoilDirection;
        private readonly float _recoilStrength;
        private readonly int _damage;
        private readonly GameObject _source;

        protected override float TotalDuration
        {
            get { return _playerSettings.HurtRecoilDuration; }
        }

        private float RecoilLength
        {
            get { return _playerSettings.HurtRecoilDistance * _recoilStrength; }
        }

        public HurtState( PlayerActor actor, Collider2D source )
        {
            _actor = actor;
            _playerSettings = PlayerSettings.Instance;            
            _recoilDirection = Mathf.Sign( _actor.transform.position.x - source.transform.position.x );

            var harmfull = source.GetInterfaceComponentInParent<IHarmfull>();
            _recoilStrength = harmfull.Recoil;
            _damage = harmfull.Damage;
            _source = harmfull.GameObject;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _actor.Health.AccountDamages( _damage, _source );
            var angle = Mathf.Atan( TotalDuration * TotalDuration * _playerSettings.Gravity / 2 / RecoilLength );
            var velocity = TotalDuration * _playerSettings.Gravity / 2 / Mathf.Sin( angle );
            _actor.Mobile.CurrentVelocity = new Vector2( _recoilDirection * Mathf.Cos( angle ), Mathf.Sin( angle ) ) * velocity;
        }

        public override IActorState Update()
        {
            var mob = _actor.Mobile;
            
            // apply gravity
            var verticalVelocity = mob.CurrentVelocity.y - _playerSettings.Gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max( verticalVelocity, -_playerSettings.MaxFallVelocity );
            mob.SetVerticalVelocity( verticalVelocity  );
            
            // default move
            mob.Move();

            return base.Update();
        }

        protected override IActorState GetNextState()
        {
            return _actor.Mobile.CheckGround() 
                ? (IActorState)new GroundedState( _actor ) 
                : new FallState( _actor );
        }
    }
}