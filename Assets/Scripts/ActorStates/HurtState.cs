using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public class HurtState : FixedTimeStateBase
    {
        private readonly float _recoilDirection;
        private readonly float _recoilStrength;
        private readonly int _damage;

        protected override float TotalDuration
        {
            get { return PlayerSettings.HurtRecoilDuration; }
        }

        private float RecoilLength
        {
            get { return PlayerSettings.HurtRecoilDistance * _recoilStrength; }
        }

        public HurtState( Actor actor, Collider2D source ) : base( actor )
        {
            _recoilDirection = Mathf.Sign( Actor.transform.position.x - source.transform.position.x );

            var harmfull = source.GetInterfaceComponent<IHarmfull>();
            _recoilStrength = harmfull.Recoil;
            _damage = harmfull.Damage;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Actor.AccountDamages( _damage );
            var angle = Mathf.Atan( TotalDuration * TotalDuration * PlayerSettings.Gravity / 2 / RecoilLength );
            var velocity = TotalDuration * PlayerSettings.Gravity / 2 / Mathf.Sin( angle );
            Actor.Mobile.CurrentVelocity = new Vector2( _recoilDirection * Mathf.Cos( angle ), Mathf.Sin( angle ) ) * velocity;
        }

        public override IActorState Update()
        {
            var mob = Actor.Mobile;
            var velocity = mob.CurrentVelocity;

            // apply gravity
            velocity.y -= PlayerSettings.Gravity * Time.deltaTime;
            velocity.y = Mathf.Max( -PlayerSettings.MaxFallVelocity, velocity.y );

            mob.CurrentVelocity = velocity;

            // default move
            mob.Move();

            return ChangeStateOnFinish();
        }
    }
}