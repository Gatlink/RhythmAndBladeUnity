using UnityEngine;

namespace ActorStates.Boss
{
    public class AttackState : BossFixedHorizontalMovementBase
    {
        private const int MaxComboCount = 2;

        public readonly int ComboCount;

        private Boss1Settings.AttackSetting _setting;

        public AttackState( BossActor actor ) : this( actor, 0 )
        {
        }

        public AttackState( BossActor actor, int comboCount ) : base( actor )
        {
            ComboCount = comboCount;
            switch ( ComboCount )
            {
                case 1:
                    _setting = Settings.Attack2;
                    break;
                // case 0:
                default:
                    _setting = Settings.Attack1;
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

        protected override AnimationCurve MovementCurve
        {
            get { return _setting.MovementCurve; }
        }

        public override IActorState Update()
        {
            ApplyHorizontalMovement();

            var time = ElapsedTime;

            if ( time > _setting.ComboWindowStartTime && time <= _setting.ComboWindowEndTime )
            {
                if ( ComboCount + 1 < MaxComboCount && Actor.CheckAttack() )
                {
                    return new AttackState( Actor, ComboCount + 1 );
                }
            }

            return base.Update();
        }
    }
}