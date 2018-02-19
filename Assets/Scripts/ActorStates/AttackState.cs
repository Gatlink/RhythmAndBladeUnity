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
        public readonly float HitDuration;

        private readonly float _comboDuration;
        private readonly float _recoveryDuration;
        private readonly float _movementLength;
        private readonly float _cooldown;
        private readonly AnimationCurve _movementCurve;


        public AttackState( Actor actor ) : this( actor, 0 )
        {
        }

        private AttackState( Actor actor, int comboCount ) : base( actor )
        {
            ComboCount = comboCount;
            switch ( ComboCount )
            {
                case 2:
                    HitDuration = PlayerSettings.Attack3HitDuration;
                    _comboDuration = 0;
                    _recoveryDuration = PlayerSettings.Attack3RecoveryDuration;
                    _movementLength = PlayerSettings.Attack3MovementLength;
                    _cooldown = PlayerSettings.Attack3Cooldown;
                    _movementCurve = PlayerSettings.Attack3MovementCurve;
                    break;
                case 1:
                    HitDuration = PlayerSettings.Attack2HitDuration;
                    _comboDuration = PlayerSettings.Attack2ComboDuration;
                    _recoveryDuration = PlayerSettings.Attack2RecoveryDuration;
                    _movementLength = PlayerSettings.Attack2MovementLength;
                    _cooldown = PlayerSettings.Attack2Cooldown;
                    _movementCurve = PlayerSettings.Attack2MovementCurve;
                    break;
                // case 0:
                default:
                    HitDuration = PlayerSettings.Attack1HitDuration;
                    _comboDuration = PlayerSettings.Attack1ComboDuration;
                    _recoveryDuration = PlayerSettings.Attack1RecoveryDuration;
                    _movementLength = PlayerSettings.Attack1MovementLength;
                    _cooldown = PlayerSettings.Attack1Cooldown;
                    _movementCurve = PlayerSettings.Attack1MovementCurve;
                    break;
            }
        }

        protected override float TotalDuration
        {
            get { return HitDuration + _comboDuration + _recoveryDuration; }
        }

        protected override float MovementLength
        {
            get { return _movementLength; }
        }

        protected override AnimationCurve MovementCurve
        {
            get { return _movementCurve; }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Actor.ConsumeAttack( _cooldown );
            _hitID = HitInfo.GenerateId();
        }

        private readonly Collider2D[] _colliderBuffer = new Collider2D[ 5 ];


        private readonly ContactFilter2D _hitContactFilter2D = new ContactFilter2D()
        {
            layerMask = 1 << LayerMask.NameToLayer( Layers.Destructible ),
            useLayerMask = true
        };

        public override IActorState Update()
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
            else if ( time > HitDuration && time <= HitDuration + _comboDuration )
            {
                // combo phase

                var harmfull = Actor.CheckDamages();
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
                    return new JumpState( Actor );
                }

                if ( Actor.CheckDash() )
                {
                    return new DashState( Actor );
                }
            }
            else
            {
                // recovery phase
                if ( !Actor.CheckGround() )
                {
                    return new FallState( Actor );
                }

                if ( Actor.CheckJump() )
                {
                    return new JumpState( Actor );
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