using UnityEngine;

namespace ActorStates.Boss
{
    public class PrepareJumpState : BossFixedTimeStateBase
    {
        private readonly Mobile _mobile;
        private bool _transitionToJumpAttack;

        public PrepareJumpState( BossActor actor, bool transitionToJumpAttack ) : base( actor )
        {
            _mobile = actor.Mobile;
            _transitionToJumpAttack = transitionToJumpAttack;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _mobile.CancelMovement();
        }

        protected override float TotalDuration
        {
            get { return Settings.PrepareJumpDuration; }
        }

        public override IActorState Update()
        {
            var hurtState = Actor.GetHurtState();
            if ( hurtState != null )
            {
                return hurtState;
            }

            return base.Update();
        }

        protected override IActorState GetNextState()
        {
            var delta = Actor.DesiredJumpMovement;
            _mobile.UpdateDirection( Mathf.Sign( delta ) );
            
            if ( _transitionToJumpAttack )
            {
                // check ceiling
                float distance,
                    jumpHeight = Settings.MaxJumpAttackHeight;
                if ( _mobile.ProbeCeiling( out distance, Settings.MaxJumpAttackHeight + 2 ) )
                {
                    jumpHeight = distance - 2;
                }

                return new JumpAttackState( Actor, jumpHeight, Mathf.Abs( delta ) );
            }
            else
            {
                return new JumpState( Actor, Settings.JumpHeight, Mathf.Abs( delta ) );
            }
        }
    }
}