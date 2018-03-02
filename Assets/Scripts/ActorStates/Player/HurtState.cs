using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates.Player
{
    public class HurtState : FixedTimeStateBase
    {
        private PlayerActor Actor;
        private PlayerSettings PlayerSettings;
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

        public HurtState( PlayerActor actor, Collider2D source )
        {
            Actor = actor;
            PlayerSettings = PlayerSettings.Instance;            
            _recoilDirection = Mathf.Sign( Actor.transform.position.x - source.transform.position.x );

            var harmfull = source.GetInterfaceComponentInParent<IHarmfull>();
            _recoilStrength = harmfull.Recoil;
            _damage = harmfull.Damage;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Actor.Health.AccountDamages( _damage );
            var angle = Mathf.Atan( TotalDuration * TotalDuration * PlayerSettings.Gravity / 2 / RecoilLength );
            var velocity = TotalDuration * PlayerSettings.Gravity / 2 / Mathf.Sin( angle );
            Actor.Mobile.CurrentVelocity = new Vector2( _recoilDirection * Mathf.Cos( angle ), Mathf.Sin( angle ) ) * velocity;
        }

        public override IActorState Update()
        {
            var mob = Actor.Mobile;
            
            // apply gravity
            var verticalVelocity = mob.CurrentVelocity.y - PlayerSettings.Gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max( verticalVelocity, -PlayerSettings.MaxFallVelocity );
            mob.SetVerticalVelocity( verticalVelocity  );
            
            // default move
            mob.Move();

            return base.Update();
        }

        protected override IActorState GetNextState()
        {
            return Actor.Mobile.CheckGround() 
                ? (IActorState)new GroundedState( Actor ) 
                : new FallState( Actor );
        }
    }
}