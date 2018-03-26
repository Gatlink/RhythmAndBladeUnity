namespace ActorStates.Boss
{
    public class AttackState : BossFixedHorizontalMovementBase
    {
        public readonly int ComboCount;

        private Boss1Settings.AttackSetting _setting;

        public AttackState( BossActor actor, int comboCount = 0 ) : base( actor )
        {
            ComboCount = comboCount;
            switch ( ComboCount )
            {
                case 0:
                    _setting = Settings.Attack1;
                    break;
                case 1:
                    _setting = Settings.Attack2;
                    break;
                default:
                    _setting = Settings.Attack3;
                    break;
            }
        }

        protected override float TotalDuration
        {
            get { return _setting.Duration; }
        }

        protected override float MovementLength
        {
            get { return _setting.HorizontalMovementLength; }
        }

        protected override Easing MovementTrajectory
        {
            get { return _setting.Trajectory; }
        }

        public override IActorState Update()
        {
            var hurtState = Actor.GetHurtState();
            if ( hurtState != null )
            {
                return hurtState;
            }

            ApplyHorizontalMovement();

            Mobile.Move();

            var time = ElapsedTime;

            if ( time > _setting.ComboWindowStartTime && time <= _setting.ComboWindowEndTime )
            {
                if ( Actor.CheckAttack() )
                {
                    return new AttackState( Actor, ComboCount + 1 );
                }
            }

            return base.Update();
        }
    }
}