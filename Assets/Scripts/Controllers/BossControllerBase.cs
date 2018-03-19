using System;
using System.Collections;
using System.Configuration;
using System.Net.Configuration;
using ActorStates;
using ActorStates.Boss;
using Gamelogic.Extensions;
using Gamelogic.Extensions.Algorithms;
using UnityEngine;

namespace Controllers
{
    public abstract class BossControllerBase : GLMonoBehaviour, IActorController<BossActor>
    {
        // ReSharper disable once NotAccessedField.Global
        // used by Editor in inspectors
        public string Name;

        public enum ActionType
        {
            Wait = 0,
            Move,
            JumpAttack,
            Charge,
            Attack,
            Count
        }

        public enum TargetType
        {
            NextToPlayer,
            LeftCorner,
            Center,
            RightCorner
        }

        protected Mobile Player;
        private Boss1Settings _settings;

        public bool Enabled
        {
            get { return enabled; }
        }

        private void ResetIntent( BossActor actor )
        {
            actor.DesiredMovement = 0;
            actor.DesiredAttack = false;
            actor.DesiredJumpAttack = false;
            actor.DesiredCharge = false;
        }

        public virtual void UpdateActorIntent( BossActor actor )
        {
            ResetIntent( actor );
        }

        private Vector2 GetHotSpotPosition( TargetType type )
        {
            throw new NotImplementedException();
        }

        // ReSharper disable once Unity.RedundantEventFunction
        protected virtual void Start()
        {
        }

        protected virtual void Awake()
        {
            Player = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Mobile>();
            _settings = Boss1Settings.Instance;
        }

        protected IEnumerator ActionResolver( BossActor actor, Action action )
        {
            switch ( action.Type )
            {
                case ActionType.Wait:
                    return WaitResolver( actor, action.DurationParameter );
                case ActionType.Move:
                    return MoveResolver( actor, action.TargetTypeParameter );
                case ActionType.JumpAttack:
                    return JumpAttackResolver( actor );
                case ActionType.Charge:
                    return ChargeResolver( actor );
                case ActionType.Attack:
                    return AttackResolver( actor, action.CountParameter );
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerator AttackResolver( BossActor actor, int count )
        {
            for ( int i = count; i > 0; i-- )
            {
                var waitState = WaitForStateEnter<AttackState>( actor );
                waitState.MoveNext();
                while ( waitState.MoveNext() )
                {
                    actor.DesiredAttack = true;
                    yield return null;
                }
            }
        }

        private IEnumerator ChargeResolver( BossActor actor )
        {
            var waitState = WaitForStateExit<ChargeAttackState>( actor );
            waitState.MoveNext();
            while ( waitState.MoveNext() )
            {
                actor.DesiredCharge = true;
                yield return null;
            }
        }

        private IEnumerator JumpAttackResolver( BossActor actor )
        {
            var waitState = WaitForStateEnter<JumpAttackState>( actor );
            waitState.MoveNext();
            while ( waitState.MoveNext() )
            {
                actor.DesiredJumpAttack = true;
                yield return null;
            }
        }

        private IEnumerator MoveResolver( BossActor actor, TargetType type )
        {
            Debug.Log( actor + " moves to " + type, actor );

            const float MoveEpsilon = 0.1f;
            var mob = actor.Mobile;

            float targetPositionX;
            if ( type == TargetType.NextToPlayer )
            {
                var toPlayer = Mathf.Sign( Player.BodyPosition.x - mob.BodyPosition.x );
                targetPositionX = Player.BodyPosition.x - _settings.CombatRangeThreshold * toPlayer;
            }
            else
            {
                targetPositionX = GetHotSpotPosition( type ).x;
            }

            var toTarget = targetPositionX - mob.BodyPosition.x;
            Debug.Log( actor + " will move " + toTarget, actor );

            if ( Mathf.Abs( toTarget ) < _settings.CloseRangeThreshold )
            {
                while ( Mathf.Abs( toTarget = targetPositionX - mob.BodyPosition.x ) > MoveEpsilon )
                {
                    actor.DesiredMovement = Mathf.Sign( toTarget );
                    yield return null;
                }
            }
            else if ( Mathf.Abs( toTarget ) < _settings.MidRangeThreshold )
            {
                var jump = WaitForStateEnter<JumpState>( actor );
                jump.MoveNext();
                while ( jump.MoveNext() )
                {
                    actor.DesiredJump = true;
                    yield return null;
                }
            }
            else
            {
                const float LongRangeWalkDistance = 1;
                var walkTargetX = mob.BodyPosition.x + Mathf.Sign( toTarget ) * LongRangeWalkDistance;
                while ( Mathf.Abs( toTarget = walkTargetX - mob.BodyPosition.x ) > MoveEpsilon )
                {
                    actor.DesiredMovement = Mathf.Sign( toTarget );
                    yield return null;
                }

                var jump = WaitForStateEnter<JumpState>( actor );
                jump.MoveNext();
                while ( jump.MoveNext() )
                {
                    actor.DesiredJump = true;
                    yield return null;
                }
            }
        }

        private IEnumerator WaitResolver( BossActor actor, float duration )
        {
            Debug.Log( actor + " waits for " + duration + " seconds", actor );

            while ( duration > 0 )
            {
                yield return null;
                duration -= Time.deltaTime;
            }
        }

        private IEnumerator WaitForStateComplete<TBossState>( BossActor actor ) where TBossState : IActorState
        {
            var waitEnter = WaitForStateEnter<TBossState>( actor );
            waitEnter.MoveNext();
            while ( waitEnter.MoveNext() )
                yield return null;
            var waitExit = WaitForStateExit<TBossState>( actor );
            waitExit.MoveNext();
            while ( waitExit.MoveNext() )
                yield return null;
        }

        private IEnumerator WaitForStateEnter<TBossState>( BossActor actor ) where TBossState : IActorState
        {
            var stateEntered = false;
            var handler = new Action<IActorState, IActorState>( ( prev, next ) =>
            {
                if ( next is TBossState )
                {
                    stateEntered = true;
                }
            } );

            actor.StateChangeEvent += handler;
            while ( !stateEntered )
            {
                yield return null;
            }

            actor.StateChangeEvent -= handler;
        }

        private IEnumerator WaitForStateExit<TBossState>( BossActor actor ) where TBossState : IActorState
        {
            var stateExited = false;
            var handler = new Action<IActorState, IActorState>( ( prev, next ) =>
            {
                if ( prev is TBossState )
                {
                    stateExited = true;
                }
            } );
            actor.StateChangeEvent += handler;
            while ( !stateExited )
            {
                yield return null;
            }

            actor.StateChangeEvent -= handler;
        }

        [ Serializable ]
        public struct Action
        {
            public ActionType Type;

            public float DurationParameter;

            public int CountParameter;

            public TargetType TargetTypeParameter;
        }

        [ Serializable ]
        public class ActionList : InspectorList<Action>
        {
        }

        private void OnDrawGizmosSelected()
        {
            var mob = GetComponent<Mobile>();
            Gizmos.color = Color.blue.Darker();
            Gizmos.DrawWireSphere( mob.BodyPosition, Boss1Settings.Instance.CombatRangeThreshold );

            Gizmos.color = Gizmos.color.Lighter();
            Gizmos.DrawWireSphere( mob.BodyPosition, Boss1Settings.Instance.CloseRangeThreshold );

            Gizmos.color = Gizmos.color.Lighter();
            Gizmos.DrawWireSphere( mob.BodyPosition, Boss1Settings.Instance.MidRangeThreshold );
        }
    }
}