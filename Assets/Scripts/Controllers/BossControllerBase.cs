﻿using System;
using System.Collections;
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
            Stands = 0,
            Move,
            Jump,
            Charge,
            Attack,
            Count
        }

        protected Mobile Player;

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

        // ReSharper disable once Unity.RedundantEventFunction
        protected virtual void Start()
        {
        }

        protected virtual void Awake()
        {
            Player = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Mobile>();
        }

        protected IEnumerator ActionResolver( BossActor actor, Action action, bool byPassRules = false )
        {
            if ( !byPassRules )
            {
                if ( action.Type == ActionType.Attack )
                {
                    // never attack if player is not in range
                    var toPlayer = Player.BodyPosition.x - actor.Mobile.BodyPosition.x;
                    if ( Mathf.Abs( toPlayer ) > Boss1Settings.Instance.CloseRangeThreshold )
                    {
                        yield break;
                    }
                }
            }

            var duration = action.SampleDuration();
            var mob = actor.Mobile;
            while ( duration > 0 )
            {
                duration -= Time.deltaTime;

                switch ( action.Type )
                {
                    case ActionType.Stands:
                        break;
                    case ActionType.Move:
                        var toPlayer = Player.BodyPosition.x - mob.BodyPosition.x;
                        if ( Mathf.Abs( toPlayer ) <= Boss1Settings.Instance.CloseRangeThreshold )
                        {
                            duration = 0;
                        }

                        actor.DesiredMovement = Mathf.Sign( toPlayer );
                        break;
                    case ActionType.Jump:
                        actor.DesiredJumpAttack = true;
                        break;
                    case ActionType.Charge:
                        actor.DesiredCharge = true;
                        break;
                    case ActionType.Attack:
                        actor.DesiredAttack = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                yield return null;
            }
        }

        private static readonly IGenerator<float> GaussianGenerator = Generator.GaussianRandomFloat( 0, 1 );

        private static float NextPositiveGaussian( float mean, float stdDev )
        {
            float next;
            do
            {
                next = GaussianGenerator.Next() * stdDev + mean;
            } while ( next < 0 );

            return next;
        }

        [ Serializable ]
        public struct Action
        {
            public ActionType Type;
            public float Duration;
            public float DurationStdDev;

            private Action( ActionType type, float duration, float stdDev = 0 )
            {
                Type = type;
                Duration = duration;
                DurationStdDev = stdDev;
            }

            public static Action Stand( float duration, float stdDev = 0 )
            {
                return new Action( ActionType.Stands, duration, stdDev );
            }

            public static Action Move( float duration, float stdDev = 0 )
            {
                return new Action( ActionType.Move, duration, stdDev );
            }

            public static Action Jump()
            {
                return new Action( ActionType.Jump, 0 );
            }

            public static Action Charge()
            {
                return new Action( ActionType.Charge, 0 );
            }

            public static Action Attack( float duration, float stdDev = 0 )
            {
                return new Action( ActionType.Attack, duration, stdDev );
            }

            public float SampleDuration()
            {
                switch ( Type )
                {
                    case ActionType.Jump:
                        return Boss1Settings.Instance.JumpAttackDuration - 0.05f;
                    case ActionType.Charge:
                        return Boss1Settings.Instance.ChargeDuration - 0.05f;
                    case ActionType.Stands:
                    case ActionType.Move:
                    case ActionType.Attack:
                    case ActionType.Count:
                    default:
                        return DurationStdDev == 0 ? Duration : NextPositiveGaussian( Duration, DurationStdDev );
                }
            }
        }

        [ Serializable ]
        public class ActionList : InspectorList<Action>
        {
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;

            var mob = GetComponent<Mobile>();
            Gizmos.DrawWireSphere( mob.BodyPosition, Boss1Settings.Instance.CloseRangeThreshold );
        }
    }
}