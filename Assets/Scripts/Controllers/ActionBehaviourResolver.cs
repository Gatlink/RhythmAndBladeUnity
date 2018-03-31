using System;
using System.Collections;
using System.Collections.Generic;
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
            BossBehaviour.Log( Time.frameCount + " " + actor + " attacks " + count + " times", actor );
            for ( var i = count; i > 0; i-- )
            {
                foreach ( var unused in _controller.WaitForStateEnter<AttackState>( actor ) )
                {
                    BossBehaviour.Log( Time.frameCount + " waiting attack state enter", actor );
                    actor.DesiredAttack = true;
                    yield return null;
                }
            }
            BossBehaviour.Log( Time.frameCount + " attack state entered", actor );
            
            foreach ( var unused in _controller.WaitForStateExit<AttackState>( actor ) )
            {
                BossBehaviour.Log( Time.frameCount + " waiting attack state exit", actor );
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
                DebugExtension.DebugArrow( mob.BodyPosition.WithX( targetPositionX - delta ), Vector3.up );

                DebugExtension.DebugArrow( mob.BodyPosition.WithX( targetPositionX ), Vector3.down );
                yield return null;
            }
        }

        private IEnumerable MoveResolver( BossActor actor, BossBehaviour.TargetType type )
        {
            BossBehaviour.Log( actor + " moves to " + type, actor );
            var mob = actor.Mobile;

            var targetPositionX = GetTargetPositionX( type, mob );

            var toTarget = targetPositionX - mob.BodyPosition.x;

            if ( Mathf.Abs( toTarget ) < _settings.CloseRangeThreshold )
            {
                foreach ( var unused in WalkResolver( actor, targetPositionX ) )
                {
                    yield return null;
                }
            }
            else if ( Mathf.Abs( toTarget ) < _settings.MidRangeThreshold )
            {
                BossBehaviour.Log( Time.frameCount + " actor will jump to targetX", actor );

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
                BossBehaviour.Log( Time.frameCount + " actor will first walk to walkTargetX", actor );
                
                foreach ( var unused in WalkResolver( actor, walkTargetX ) )
                {
                    yield return null;
                }
                
                BossBehaviour.Log( Time.frameCount + " then actor will jump to targetX", actor );
                foreach ( var unused in JumpAndWaitForCompletion( actor, targetPositionX - mob.BodyPosition.x ) )
                {
                    DebugExtension.DebugArrow( mob.BodyPosition.WithX( targetPositionX ), Vector3.down );

                    yield return null;
                }
            }
        }

        private static IEnumerable WalkResolver( BossActor actor, float targetPositionX )
        {
            var mob = actor.Mobile;
            const float MovementEpsilon = 0.1f;
            
            // set a timeout to prevent player from using properly timed attacks in order to block movement
            var walkSpeed = Boss1Settings.Instance.GroundedMovementSpeed;
            var distance = Mathf.Abs( mob.BodyPosition.x - targetPositionX );
            var duration = distance / walkSpeed;

            BossBehaviour.Log( Time.frameCount + " actor will walk to targetX", actor );

            while ( Mathf.Abs( targetPositionX - mob.BodyPosition.x ) > MovementEpsilon && duration > 0)
            {
                DebugExtension.DebugArrow( mob.BodyPosition.WithX( targetPositionX ), Vector3.down );
                BossBehaviour.Log( Time.frameCount + " wait for actor to reach targetX", actor );

                duration -= Time.deltaTime;
                actor.DesiredMovement = Mathf.Sign( targetPositionX - mob.BodyPosition.x );
                yield return null;
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
            foreach ( var unused in _controller.WaitForStateEnter<ChargeAttackState>( actor ) )
            {
                BossBehaviour.Log( Time.frameCount + " wait for charge state enter", actor );
                actor.DesiredCharge = true;
                yield return null;
            }
            BossBehaviour.Log( Time.frameCount + " charge state entered", actor );
            foreach ( var unused in _controller.WaitForStateExit<ChargeAttackState>( actor ) )
            {
                BossBehaviour.Log( Time.frameCount + " wait for charge state exit", actor );
                yield return null;
            }
            BossBehaviour.Log( Time.frameCount + " charge state exited", actor );
        }

        private IEnumerable JumpAttackAndWaitForCompletion( BossActor actor, float toTarget )
        {
            foreach ( var unused in _controller.WaitForStateEnter<JumpAttackState>( actor ) )
            {
                BossBehaviour.Log( Time.frameCount + " wait for jumpattack state enter", actor );
                actor.DesiredJumpAttack = true;
                actor.DesiredJumpMovement = toTarget;
                yield return null;
            }
            BossBehaviour.Log( Time.frameCount + " jumpattack state entered", actor );
            foreach ( var unused in _controller.WaitForStateExit<StrikeGroundState>( actor ) )
            {
                BossBehaviour.Log( Time.frameCount + " wait for strike ground state exit", actor );
                yield return null;
            }
            BossBehaviour.Log( Time.frameCount + " strike ground state exited", actor );
        }

        private IEnumerable JumpAndWaitForCompletion( BossActor actor, float toTarget )
        {
            foreach ( var unused in _controller.WaitForStateEnter<JumpState>( actor ) )
            {
                BossBehaviour.Log( Time.frameCount + " wait for jump state enter", actor );
                actor.DesiredJump = true;
                actor.DesiredJumpMovement = toTarget;
                yield return null;
            }
            BossBehaviour.Log( Time.frameCount + " jump state entered", actor );

            foreach ( var unused in _controller.WaitForStateExit<JumpState>( actor ) )
            {
                BossBehaviour.Log( Time.frameCount + " wait for jump state exit", actor );
                yield return null;
            }
            BossBehaviour.Log( Time.frameCount + " jump state exited", actor );
        }
    }
}