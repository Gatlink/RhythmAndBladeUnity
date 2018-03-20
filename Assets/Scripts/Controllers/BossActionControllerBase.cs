using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ActorStates;
using ActorStates.Boss;
using Gamelogic.Extensions;
using UnityEngine;

namespace Controllers
{
    public abstract class BossActionControllerBase : BossControllerBase
    {
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
        private Dictionary<TargetType, Vector2> _hotSpotsPositions;
                
        private void ResetIntent( BossActor actor )
        {
            actor.DesiredMovement = 0;
            actor.DesiredAttack = false;
            actor.DesiredJumpAttack = false;
            actor.DesiredJump = false;
            actor.DesiredCharge = false;
        }

        public override void UpdateActorIntent( BossActor actor )
        {
            base.UpdateActorIntent( actor );
            ResetIntent( actor );
        }

        private Vector2 GetHotSpotPosition( TargetType type )
        {
            return _hotSpotsPositions[ type ];
        }

        // ReSharper disable once Unity.RedundantEventFunction
        protected virtual void Start()
        {
            _hotSpotsPositions = GameObject.FindGameObjectsWithTag( Tags.HotSpot )
                .ToDictionary(
                    go => (TargetType) Array.FindIndex( Enum.GetNames( typeof( TargetType ) ),
                        s => s.Equals( go.name ) ),
                    go => (Vector2) go.transform.position );
            if ( _hotSpotsPositions.Count < 3 )
            {
                Debug.LogError( "Missing hot spots!" );
            }
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
#if DEBUG_CONTROLLER_ACTION
            Debug.Log( actor + " attacks " + count + " times", actor );
#endif

            for ( int i = count; i > 0; i-- )
            {
                foreach ( var unused in WaitForStateEnter<AttackState>( actor ) )
                {
                    actor.DesiredAttack = true;
                    yield return null;
                }
            }

            foreach ( var unused in WaitForStateExit<AttackState>( actor ) )
            {
                yield return null;
            }
        }

        private IEnumerator ChargeResolver( BossActor actor )
        {
#if DEBUG_CONTROLLER_ACTION
            Debug.Log( actor + " charges", actor );
#endif
            foreach ( var unused in ChargeAndWaitForCompletion( actor ) )
            {
                yield return null;
            }
        }

        private IEnumerator JumpAttackResolver( BossActor actor )
        {
#if DEBUG_CONTROLLER_ACTION
            Debug.Log( actor + " jump attacks", actor );
#endif
            var target = Mathf.Clamp( Player.BodyPosition.x, GetHotSpotPosition( TargetType.LeftCorner ).x,
                GetHotSpotPosition( TargetType.RightCorner ).x ); 
            var delta = target - actor.Mobile.BodyPosition.x;            

            foreach ( var unused in JumpAttackAndWaitForCompletion( actor, delta ) )
            {
                yield return null;
            }
        }

        private IEnumerator MoveResolver( BossActor actor, TargetType type )
        {
#if DEBUG_CONTROLLER_ACTION
            Debug.Log( actor + " moves to " + type, actor );
#endif

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
            DebugExtension.DebugArrow( mob.BodyPosition.WithX( targetPositionX ), Vector3.down, 1 );

            if ( Mathf.Abs( toTarget ) < _settings.CloseRangeThreshold )
            {
                var movementDirection = Mathf.Sign( toTarget );
                while ( Mathf.Sign( targetPositionX - mob.BodyPosition.x ) * movementDirection > 0 )
                {
                    actor.DesiredMovement = movementDirection;
                    yield return null;
                }
            }
            else if ( Mathf.Abs( toTarget ) < _settings.MidRangeThreshold )
            {
                foreach ( var unused in JumpAndWaitForCompletion( actor, toTarget ) )
                {
                    yield return null;
                }
            }
            else
            {
                const float LongRangeWalkDistance = 2;
                var movementDirection = Mathf.Sign( toTarget );
                var walkTargetX = mob.BodyPosition.x + movementDirection * LongRangeWalkDistance;
                while ( Mathf.Sign( walkTargetX - mob.BodyPosition.x ) * movementDirection > 0 )
                {
                    actor.DesiredMovement = movementDirection;
                    yield return null;
                }

                foreach ( var unused in JumpAndWaitForCompletion( actor, targetPositionX - mob.BodyPosition.x ) )
                {
                    yield return null;
                }
            }
        }

        private IEnumerator WaitResolver( BossActor actor, float duration )
        {
#if DEBUG_CONTROLLER_ACTION
            Debug.Log( actor + " waits for " + duration + " seconds", actor );
#endif

            while ( duration > 0 )
            {
                yield return null;
                duration -= Time.deltaTime;
            }
        }

        private IEnumerable ChargeAndWaitForCompletion( BossActor actor )
        {
            foreach ( var unused in WaitForStateEnter<ChargeAttackState>( actor ) )
            {
                actor.DesiredCharge = true;
                yield return null;
            }

            foreach ( var unused in WaitForStateExit<ChargeAttackState>( actor ) )
            {
                yield return null;
            }
        }
        
        private IEnumerable JumpAttackAndWaitForCompletion( BossActor actor, float toTarget )
        {
            foreach ( var unused in WaitForStateEnter<JumpAttackState>( actor ) )
            {
                actor.DesiredJumpAttack = true;
                actor.DesiredJumpMovement = toTarget;
                yield return null;
            }

            foreach ( var unused in WaitForStateExit<StrikeGroundState>( actor ) )
            {
                yield return null;
            }
        }

        private IEnumerable JumpAndWaitForCompletion( BossActor actor, float toTarget )
        {
            foreach ( var unused in WaitForStateEnter<JumpState>( actor ) )
            {
                actor.DesiredJump = true;
                actor.DesiredJumpMovement = toTarget;
                yield return null;
            }

            foreach ( var unused in WaitForStateExit<JumpState>( actor ) )
            {
                yield return null;
            }
        }

        private IEnumerable WaitForStateEnter<TBossState>( BossActor actor ) where TBossState : IActorState
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

        private IEnumerable WaitForStateExit<TBossState>( BossActor actor ) where TBossState : IActorState
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
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere( mob.BodyPosition, Boss1Settings.Instance.CombatRangeThreshold );

            Gizmos.color = Gizmos.color.Lighter().Lighter().Lighter();
            Gizmos.DrawWireSphere( mob.BodyPosition, Boss1Settings.Instance.CloseRangeThreshold );

            Gizmos.color = Gizmos.color.Lighter().Lighter().Lighter();
            Gizmos.DrawWireSphere( mob.BodyPosition, Boss1Settings.Instance.MidRangeThreshold );
        }
    }
}