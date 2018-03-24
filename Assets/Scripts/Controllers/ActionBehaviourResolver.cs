using System;
using System.Collections;
using ActorStates;
using ActorStates.Boss;
using Gamelogic.Extensions;
using UnityEngine;

namespace Controllers
{
    public class ActionBehaviourResolver
    {
        private readonly BossBehaviourController _controller;
        private readonly Boss1Settings _settings;
        
        public ActionBehaviourResolver( BossBehaviourController controller )
        {
            _controller = controller;
            _settings = Boss1Settings.Instance;
        }
               
        public IEnumerable GetResolver( ActionBehaviourNode node )
        {
            var actor = _controller.GetComponent<BossActor>();
         
            BossBehaviour.Log( actor + " starts behaviour " + node.Name, actor );
            foreach ( var action in node.Script )
            {
                foreach ( var unused in ActionResolver( actor, action ) )
                {
                    yield return null;
                }
            }
        }

        private float ClampPositionX( float x )
        {
            var left = _controller.GetHotSpotPosition( BossBehaviour.TargetType.LeftCorner ).x;
            var right = _controller.GetHotSpotPosition( BossBehaviour.TargetType.RightCorner ).x;
            return Mathf.Clamp( x, left, right );
        }

        private IEnumerable ActionResolver( BossActor actor, BossBehaviour.Action action )
        {
            switch ( action.Type )
            {
                case BossBehaviour.ActionType.Wait:
                    return WaitResolver( actor, action.DurationParameter );
                case BossBehaviour.ActionType.Move:
                    return MoveResolver( actor, action.TargetTypeParameter );
                case BossBehaviour.ActionType.JumpAttack:
                    return JumpAttackResolver( actor, action.TargetTypeParameter );
                case BossBehaviour.ActionType.Charge:
                    return ChargeResolver( actor );
                case BossBehaviour.ActionType.Attack:
                    return AttackResolver( actor, action.CountParameter );
                case BossBehaviour.ActionType.Count:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerable AttackResolver( BossActor actor, int count )
        {
            BossBehaviour.Log( actor + " attacks " + count + " times", actor );
            for ( var i = count; i > 0; i-- )
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

        private IEnumerable ChargeResolver( BossActor actor )
        {
            BossBehaviour.Log( actor + " charges", actor );
            foreach ( var unused in ChargeAndWaitForCompletion( actor ) )
            {
                yield return null;
            }
        }

        private IEnumerable JumpAttackResolver( BossActor actor, BossBehaviour.TargetType type )
        {
            BossBehaviour.Log( actor + " jump attacks", actor );
            var mob = actor.Mobile;

            var targetPositionX = GetTargetPositionX( type, mob );
//            var targetPositionX = Mathf.Clamp( Player.BodyPosition.x, GetHotSpotPosition( TargetType.LeftCorner ).x,
//                GetHotSpotPosition( TargetType.RightCorner ).x );
            var delta = targetPositionX - actor.Mobile.BodyPosition.x;

            foreach ( var unused in JumpAttackAndWaitForCompletion( actor, delta ) )
            {
                yield return null;
            }
        }

        private IEnumerable MoveResolver( BossActor actor, BossBehaviour.TargetType type )
        {
            BossBehaviour.Log( actor + " moves to " + type, actor );
            const float MovementEpsilon = 0.1f;
            var mob = actor.Mobile;

            var targetPositionX = GetTargetPositionX( type, mob );

            var toTarget = targetPositionX - mob.BodyPosition.x;

            if ( Mathf.Abs( toTarget ) < _settings.CloseRangeThreshold )
            {
                while ( Mathf.Abs( targetPositionX - mob.BodyPosition.x ) > MovementEpsilon )
                {
                    DebugExtension.DebugArrow( mob.BodyPosition.WithX( targetPositionX ), Vector3.down );

                    actor.DesiredMovement = Mathf.Sign( targetPositionX - mob.BodyPosition.x );
                    yield return null;
                }
            }
            else if ( Mathf.Abs( toTarget ) < _settings.MidRangeThreshold )
            {
                foreach ( var unused in JumpAndWaitForCompletion( actor, toTarget ) )
                {
                    DebugExtension.DebugArrow( mob.BodyPosition.WithX( targetPositionX ), Vector3.down );

                    yield return null;
                }
            }
            else
            {
                const float LongRangeWalkDistance = 2;
                var movementDirection = Mathf.Sign( toTarget );
                var walkTargetX = ClampPositionX( mob.BodyPosition.x + movementDirection * LongRangeWalkDistance );
                while ( Mathf.Abs( walkTargetX - mob.BodyPosition.x ) > MovementEpsilon )
                {
                    DebugExtension.DebugArrow( mob.BodyPosition.WithX( walkTargetX ), Vector3.down );

                    actor.DesiredMovement = Mathf.Sign( walkTargetX - mob.BodyPosition.x );
                    yield return null;
                }

                foreach ( var unused in JumpAndWaitForCompletion( actor, targetPositionX - mob.BodyPosition.x ) )
                {
                    DebugExtension.DebugArrow( mob.BodyPosition.WithX( targetPositionX ), Vector3.down );

                    yield return null;
                }
            }
        }

        private float GetTargetPositionX( BossBehaviour.TargetType type, Mobile mob )
        {
            switch ( type )
            {
                case BossBehaviour.TargetType.NextToPlayer:
                    var toPlayer = Mathf.Sign( _controller.Player.BodyPosition.x - mob.BodyPosition.x );
                    return ClampPositionX(
                        _controller.Player.BodyPosition.x - _settings.CombatRangeThreshold * toPlayer );

                case BossBehaviour.TargetType.FarthestCornerFromPlayer:
                    var left = _controller.GetHotSpotPosition( BossBehaviour.TargetType.LeftCorner ).x;
                    var right = _controller.GetHotSpotPosition( BossBehaviour.TargetType.RightCorner ).x;
                    var player = _controller.Player.BodyPosition.x;
                    return Mathf.Abs( player - left ) < Mathf.Abs( player - right ) ? right : left;

                case BossBehaviour.TargetType.OntoPlayer:
                    return ClampPositionX( _controller.Player.BodyPosition.x );

                case BossBehaviour.TargetType.LeftPlatform:
                case BossBehaviour.TargetType.RightPlatform:
                case BossBehaviour.TargetType.LeftCorner:
                case BossBehaviour.TargetType.Center:
                case BossBehaviour.TargetType.RightCorner:
                default:
                    return _controller.GetHotSpotPosition( type ).x;
            }
        }

        private IEnumerable WaitResolver( BossActor actor, float duration )
        {
            BossBehaviour.Log( actor + " waits for " + duration + " seconds", actor );
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
    }
}