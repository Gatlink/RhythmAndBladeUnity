using System.Linq;
using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public class AttackState : FixedHorizontalMovementStateBase
    {
        private const int MaxComboCount = 3;
        private uint _hitID;

        public readonly int ComboCount;

        public float HitDuration
        {
            get { return _setting.HitDuration; }
        }

        private AttackSetting _setting;
        
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

            _hitContactFilter2D = new ContactFilter2D();
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            _hitContactFilter2D.NoFilter();
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            _hitContactFilter2D.SetLayerMask( 1 << LayerMask.NameToLayer( Layers.Destructible ) );
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
            _hitID = HitInfo.GenerateId();
        }

        private readonly Collider2D[] _colliderBuffer = new Collider2D[ 5 ];


        private readonly ContactFilter2D _hitContactFilter2D;

        public override IActorState<PlayerActor> Update()
        {
            ApplyHorizontalMovement();

            var time = ElapsedTime;

            if ( time <= HitDuration )
            {
                // hit phase
                foreach ( var hitbox in Actor.GetComponentsInChildren<Collider2D>()
                    .Where( collider => collider.CompareTag( Tags.Hitbox ) && collider.enabled ) )
                {
                    var hitCount = hitbox.OverlapCollider( _hitContactFilter2D, _colliderBuffer );
                    if ( hitCount > 0 )
                    {
                        for ( int i = 0; i < hitCount; i++ )
                        {
                            var colliderHit = _colliderBuffer[ i ];
                            var destructible = colliderHit.GetInterfaceComponent<IDestructible>();
                            if ( destructible != null )
                            {
                                destructible.Hit( new HitInfo( _hitID ) );
                            }
                        }
                    }
                }
            }
            else if ( time > HitDuration && time <= HitDuration + _setting.ComboDuration )
            {
                // combo phase

                var harmfull = Actor.Health.CheckDamages();
                if ( harmfull != null )
                {
                    return new HurtState( Actor, harmfull );
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

            return ChangeStateOnFinish();
        }
    }
}