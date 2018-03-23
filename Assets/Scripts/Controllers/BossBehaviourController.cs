using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions;
using UnityEngine;

namespace Controllers
{
    public class BossBehaviourController : GLMonoBehaviour, IActorController<BossActor>
    {
        public BossBehaviour Behaviour;

        private IEnumerator _mainResolverEnumerator;

        [ HideInInspector ]
        public Mobile Player;

        private Dictionary<BossBehaviour.TargetType, Vector2> _hotSpotsPositions;

        private void Start()
        {
            Player = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Mobile>();
            _hotSpotsPositions = GameObject.FindGameObjectsWithTag( Tags.HotSpot )
                .ToDictionary(
                    go => (BossBehaviour.TargetType) Array.FindIndex(
                        Enum.GetNames( typeof( BossBehaviour.TargetType ) ),
                        s => s.Equals( go.name ) ),
                    go => (Vector2) go.transform.position );
            if ( _hotSpotsPositions.Count < 3 )
            {
                Debug.LogError( "Missing hot spots!" );
            }
        }

        public Vector2 GetHotSpotPosition( BossBehaviour.TargetType type )
        {
            return _hotSpotsPositions[ type ];
        }

        public bool Enabled
        {
            get { return enabled; }
        }

        private void OnEnable()
        {
            _mainResolverEnumerator = GetBehaviourNodeResolver( Behaviour.MainBehaviour ).GetEnumerator();
        }

        public void UpdateActorIntent( BossActor actor )
        {
            ResetIntent( actor );
            if ( _mainResolverEnumerator != null )
            {
                _mainResolverEnumerator.MoveNext();
            }
        }

        public void ResetIntent( BossActor actor )
        {
            actor.DesiredMovement = 0;
            actor.DesiredAttack = false;
            actor.DesiredJumpAttack = false;
            actor.DesiredJump = false;
            actor.DesiredCharge = false;
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

        public IEnumerable GetBehaviourNodeResolver( BehaviourNode node )
        {
            var actionBehaviourNode = node as ActionBehaviourNode;
            if ( actionBehaviourNode != null )
            {
                return new ActionBehaviourResolver( this ).GetResolver( actionBehaviourNode );
            }

            var compoundBehaviourNode = node as CompoundBehaviourNode;
            if ( compoundBehaviourNode != null )
            {
                return new CompoundBehaviourResolver( this ).GetResolver( compoundBehaviourNode );
            }

            throw new ArgumentException( "Invalid behaviour node type" );
        }

        public IEnumerable GetBehaviourNodeResolver( string nodeGuid )
        {
            return GetBehaviourNodeResolver( Behaviour.GetBehaviourNode( nodeGuid ) );
        }
    }
}