using UnityEngine;

namespace ActorStates.Boss
{
    public class PrepareJump : BossFixedTimeStateBase
    {
        private readonly Mobile _mobile;

        public PrepareJump( BossActor actor ) : base( actor )
        {
            _mobile = actor.Mobile;
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

        protected override IActorState GetNextState()
        {
            // check ceiling                
            float distance,
                jumpHeight = Settings.MaxJumpAttackHeight;
            if ( _mobile.ProbeCeiling( out distance, Settings.MaxJumpAttackHeight + 2 ) )
            {
                jumpHeight = distance - 2;
            }

            var jumpDistance = 5; // todo compute good target

            return new JumpAttackState( Actor, jumpHeight,
                Mathf.Min( jumpDistance, Settings.MaxJumpAttackMovementLength ) );
        }
    }
}