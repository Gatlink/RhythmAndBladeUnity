using UnityEngine;

namespace ActorStates.Player
{
    public class AttackState : PlayerFixedHorizontalMovementState
    {
        private const int MaxComboCount = 3;
        private uint _hitId;

        public readonly int ComboCount;

        public float HitDuration
        {
            get { return _setting.HitDuration; }
        }

        private PlayerSettings.AttackSetting _setting;
        
        public AttackState( PlayerActor actor ) : this( actor, 0 )
        {
        }

        private AttackState( PlayerActor actor, int comboCount ) : base( actor )
        {
            ComboCount = comboCount;
            switch ( ComboCount )
            {
                case 2:
                    _setting = PlayerSettings.Attack3;
                    break;
                case 1:
                    _setting = PlayerSettings.Attack2;
                    break;
                // case 0:
                default:
                    _setting = PlayerSettings.Attack1;
                    break;
            }
        }

        protected override float TotalDuration
        {
            get { return HitDuration + _setting.ComboDuration + _setting.RecoveryDuration; }
        }

        protected override float MovementLength
        {
            get { return _setting.HorizontalMovementLength; }
        }

        protected override AnimationCurve MovementCurve
        {
            get { return _setting.MovementCurve; }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Actor.ConsumeAttack( _setting.Cooldown );
            _hitId = HitInfo.GenerateId();
        }

        public override IActorState Update()
        {
            ApplyHorizontalMovement();

            var time = ElapsedTime;

            if ( time <= HitDuration )
            {
                // hit phase
                Actor.DealDamage( _hitId );
            }
            else if ( time > HitDuration && time <= HitDuration + _setting.ComboDuration )
            {
                // combo phase

                var harmfull = Actor.CheckDamages();
                if ( harmfull != null )
                {
                    return harmfull;
                }

                if ( ComboCount + 1 < MaxComboCount
                     && Actor.CheckAttack( isCombo: true ) )
                {
                    return new AttackState( Actor, ComboCount + 1 );
                }

                if ( Actor.CheckJump() )
                {
                    return new JumpState( Actor, PlayerSettings.NormalJump );
                }

                if ( Actor.CheckDash() )
                {
                    return new DashState( Actor );
                }
            }
            else
            {
                // recovery phase
                if ( !Actor.Mobile.CheckGround() )
                {
                    return new FallState( Actor );
                }

                if ( Actor.CheckJump() )
                {
                    return new JumpState( Actor, PlayerSettings.NormalJump );
                }

                if ( Actor.CheckDash() )
                {
                    return new DashState( Actor );
                }
            }

            return base.Update();
        }
    }
}