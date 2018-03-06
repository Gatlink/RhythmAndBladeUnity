using UnityEngine;

namespace ActorStates.Boss
{
    public class JumpAttackState : BossFixedVerticalMovementStateBase
    {
        private readonly float _jumpHeight;

        public JumpAttackState( BossActor actor, float height ) : base( actor )
        {
            _jumpHeight = height;
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
            get { return _jumpHeight; }
        }

        public override IActorState Update()
        {
            ApplyVerticalMovement();

            // default move
            Mobile.Move();

            return base.Update();
        }

        protected override IActorState GetNextState()
        {
            return new DiveState( Actor );
        }
    }
}