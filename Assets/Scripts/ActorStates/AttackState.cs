using System.Linq;
using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public class AttackState : FixedHorizontalMovementStateBase
    {
        private const int MaxComboCount = 3;

        private readonly int _comboCount;
        public readonly float HitDuration;
        private readonly float _comboDuration;
        private readonly float _recoveryDuration;
        private readonly float _movementLength;
        private readonly float _coolDown;
        private readonly AnimationCurve _movementCurve;

        public AttackState( Actor actor ) : this( actor, 0 )
        {
        }

        private AttackState( Actor actor, int comboCount ) : base( actor )
        {
            _comboCount = comboCount;
            switch ( _comboCount )
            {
                case 2:
                    HitDuration = PlayerSettings.Attack3HitDuration;
                    _comboDuration = 0;
                    _recoveryDuration = PlayerSettings.Attack3RecoveryDuration;
                    _movementLength = PlayerSettings.Attack3MovementLength;
                    _coolDown = PlayerSettings.Attack3Cooldown;
                    _movementCurve = PlayerSettings.Attack3MovementCurve;
                    break;
                case 1:
                    HitDuration = PlayerSettings.Attack2HitDuration;
                    _comboDuration = PlayerSettings.Attack2ComboDuration;
                    _recoveryDuration = PlayerSettings.Attack2RecoveryDuration;
                    _movementLength = PlayerSettings.Attack2MovementLength;
                    _coolDown = PlayerSettings.Attack2Cooldown;
                    _movementCurve = PlayerSettings.Attack2MovementCurve;
                    break;
                // case 0:
                default:
                    HitDuration = PlayerSettings.Attack1HitDuration;
                    _comboDuration = PlayerSettings.Attack1ComboDuration;
                    _recoveryDuration = PlayerSettings.Attack1RecoveryDuration;
                    _movementLength = PlayerSettings.Attack1MovementLength;
                    _coolDown = PlayerSettings.Attack1Cooldown;
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
            Actor.AttackCooldown = _coolDown;
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
                    Debug.Log( this + " testing hitbox " + hitbox, hitbox );
                    var hitCount = hitbox.OverlapCollider( _hitContactFilter2D, _colliderBuffer );
                    if ( hitCount > 0 )
                    {
                        for ( int i = 0; i < hitCount; i++ )
                        {
                            var colliderHit = _colliderBuffer[ i ];
                            Debug.Log( hitbox + " hit " + colliderHit, colliderHit );
                            var destructible = colliderHit.GetInterfaceComponent<IDestructible>();
                            if ( destructible != null )
                            {
                                destructible.Hit( Actor.gameObject );
                            }
                        }
                    }
                }
            }
            else
            {
                if ( time > HitDuration
                     && time <= HitDuration + _comboDuration
                     && _comboCount + 1 < MaxComboCount
                     && Actor.CheckAttack( ignoreCoolDown: true ) )
                {
                    // combo phase
                    return new AttackState( Actor, _comboCount + 1 );
                }

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