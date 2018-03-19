using System;
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
            Wait = 0,
            Move,
            JumpAttack,
            Charge,
            Attack,
            Count
        }

        public enum TargetPosition
        {
            LeftCorner,
            Center,
            RightCorner,
            NextToPlayer
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

        protected IEnumerator ActionResolver( BossActor actor, Action action )
        {
            switch ( action.Type )
            {
                case ActionType.Wait:
                    return ResolveWait( actor, action.DurationParameter );
                case ActionType.Move:
                    return ResolveMove( actor, action.TargetPositionParameter );
                case ActionType.JumpAttack:
                    return ResolveJumpAttack( actor );
                case ActionType.Charge:
                    return ResolveCharge( actor );
                case ActionType.Attack:
                    return ResolveAttack( actor, action.CountParameter );
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerator ResolveAttack( BossActor actor, int count )
        {
            throw new NotImplementedException();
        }

        private IEnumerator ResolveCharge( BossActor actor )
        {
            throw new NotImplementedException();
        }

        private IEnumerator ResolveJumpAttack( BossActor actor )
        {
            throw new NotImplementedException();
        }

        private IEnumerator ResolveMove( BossActor actor, TargetPosition position )
        {
            throw new NotImplementedException();
        }

        private IEnumerator ResolveWait( BossActor actor, float duration )
        {
            throw new NotImplementedException();
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

            public float DurationParameter;

            public int CountParameter;

            public TargetPosition TargetPositionParameter;
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