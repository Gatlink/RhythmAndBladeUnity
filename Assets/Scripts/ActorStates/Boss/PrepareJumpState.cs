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

        protected override IActorState GetNextState()
        {
            if ( _transitionToJumpAttack )
            {
                var playerPosition =
                    GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Mobile>().BodyPosition;
                var delta = playerPosition.x - _mobile.BodyPosition.x;

                _mobile.UpdateDirection( Mathf.Sign( delta ) );

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
                var delta = Actor.DesiredJumpMovement;
                _mobile.UpdateDirection( Mathf.Sign( delta ) );

                return new JumpState( Actor, Settings.JumpHeight, Mathf.Abs( delta ) );
            }
        }
    }
}