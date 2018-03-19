namespace ActorStates.Boss
{
    public class JumpAttackState : JumpState
    {
        public JumpAttackState( BossActor actor, float height, float distance ) : base( actor, height, distance )
        {
        }

        protected override float TotalDuration
        {
            get { return Settings.JumpAttackDuration; }
        }

        protected override Easing MovementTrajectory
        {
            get { return Settings.JumpAttackHeightTrajectory; }
        }

        protected override Easing HorizontalMovementTrajectory
        {
            get { return Settings.JumpAttackMovementTrajectory; }
        }

        protected override IActorState GetNextState()
        {
            return new DiveState( Actor );
        }
    }
}