using System;
using System.Collections;
using Gamelogic.Extensions;
using Gamelogic.Extensions.Algorithms;
using UnityEngine;

namespace Controllers
{
    public abstract class BossControllerBase : GLMonoBehaviour, IActorController<BossActor>
    {
        public float CloseRangeThreshold;

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

        public abstract void UpdateActorIntent( BossActor actor );

        // ReSharper disable once Unity.RedundantEventFunction
        protected virtual void Start()
        {            
        }

        protected virtual void Awake()
        {
            Player = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Mobile>();
        }

        protected IEnumerator ActionResolver( BossActor actor, Action action )
        {
            var duration = action.DurationStdDev == 0
                ? action.Duration
                : NextPositiveGaussian( action.Duration, action.DurationStdDev );

            while ( duration > 0 )
            {
                duration -= Time.deltaTime;

                switch ( action.Type )
                {
                    case ActionType.Stands:
                        break;
                    case ActionType.Move:
                        var toPlayer = Player.BodyPosition - actor.Mobile.BodyPosition;
                        if ( Mathf.Abs( toPlayer.x ) <= CloseRangeThreshold )
                        {
                            duration = 0;
                        }

                        actor.DesiredMovement = Mathf.Sign( toPlayer.x );
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
                return new Action( ActionType.Jump, Boss1Settings.Instance.JumpAttackDuration );
            }

            public static Action Charge()
            {
                return new Action( ActionType.Charge, Boss1Settings.Instance.ChargeDuration );
            }

            public static Action Attack( float duration, float stdDev = 0 )
            {
                return new Action( ActionType.Attack, duration, stdDev );
            }
        }
        
        [Serializable]
        public class ActionList : InspectorList<Action>
        {
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;

            var mob = GetComponent<Mobile>();
            Gizmos.DrawWireSphere( mob.BodyPosition, CloseRangeThreshold );
        }
    }
}