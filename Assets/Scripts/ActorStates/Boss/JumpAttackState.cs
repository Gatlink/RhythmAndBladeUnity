using UnityEngine;

namespace ActorStates.Boss
{
    public class JumpAttackState : BossFixedVerticalMovementStateBase
    {
        // normalized time before wich ground is not checked yet
        private const float GroundCheckInhibitionTime = 1f;

        public JumpAttackState( BossActor actor ) : base( actor )
        {            
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Mobile.CancelHorizontalMovement();
        }

        protected override float TotalDuration
        {
            get { return Settings.JumpAttackDuration; }
        }

        protected override AnimationCurve MovementCurve
        {
            get { return Settings.JumpAttackHeightCurve; }
        }

        protected override float MovementLength
        {
            get { return Settings.JumpAttackHeight; }
        }

        public override IActorState Update()
        {
            ApplyVerticalMovement();

            // default move
            Mobile.Move();

            if ( NormalizedTime > GroundCheckInhibitionTime && Mobile.CheckGround() )
            {
                return new GroundedState( Actor );
            }

            return base.Update();
        }
    }
}