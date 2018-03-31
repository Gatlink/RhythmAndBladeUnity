using UnityEngine;

namespace ActorStates.Player
{
    public class AttackState : PlayerFixedTimeStateBase
    {
        private const int MaxComboCount = 3;
        private uint _hitId;

        public readonly int ComboCount;

        public float HitDuration
        {
            get { return _setting.HitDuration; }
        }

        private PlayerSettings.AttackSetting _setting;
        private Vector2 _clampedVelocityOnEnter;

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

        public override void OnEnter()
        {
            base.OnEnter();
            _hitId = HitInfo.GenerateId();

            Actor.AddAttackCooldown( _setting.Cooldown );
            
            Actor.Mobile.UpdateDirection( Actor.DesiredMovement );

            _clampedVelocityOnEnter = Actor.Mobile.CurrentVelocity.ClampedMagnitude( PlayerSettings.MaxAirAttackVelocity );
        }

        public override IActorState Update()
        {
            var mob = Actor.Mobile;

            var grounded = Actor.Mobile.CheckGround();
            if ( !grounded )
            {
                var inertia = mob.CurrentVelocity.y > 0
                    ? PlayerSettings.AirAttackMoveUpwardInertia
                    : PlayerSettings.AirAttackMoveDownwardInertia; 
                mob.ChangeVelocity( _clampedVelocityOnEnter, inertia );                
            }
            else
            {
                mob.ChangeHorizontalVelocity( 0, PlayerSettings.AttackMoveInertia );
            }
            
            mob.Move();

            var time = ElapsedTime;

            if ( time <= HitDuration )
            {
                // hit phase
                Actor.CheckHits( _hitId );
            }
            else if ( time > HitDuration && time <= HitDuration + _setting.ComboDuration )
            {
                // combo phase

                var harmfull = Actor.CheckHurts();
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