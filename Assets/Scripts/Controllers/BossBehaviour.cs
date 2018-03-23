using System;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{
    [ CreateAssetMenu ]
    public class BossBehaviour : ScriptableObject
    {
        [ SerializeField ]
        private ActionBehaviourDictionary _actionBehaviours = new ActionBehaviourDictionary();

        [ SerializeField ]
        private CompoundBehavioursDictionary _compoundBehaviours = new CompoundBehavioursDictionary();

        [ HideInInspector ]
        public string MainBehaviourGuid;

        public BehaviourNode MainBehaviour
        {
            get { return GetBehaviourNode( MainBehaviourGuid ); }
            set { MainBehaviourGuid = value.Guid; }
        }

        public IEnumerable<BehaviourNode> GetAllBehaviourNodes()
        {
            foreach ( var node in _actionBehaviours.Values )
            {
                yield return node;
            }

            foreach ( var node in _compoundBehaviours.Values )
            {
                yield return node;
            }
        }

        public IEnumerable<ActionBehaviourNode> GetActionBehaviourNodes()
        {
            return _actionBehaviours.Values;
        }

        public IEnumerable<CompoundBehaviourNode> GetCompoundBehaviourNodes()
        {
            return _compoundBehaviours.Values;
        }

        public int TotalNodeCount
        {
            get { return _actionBehaviours.Count + _compoundBehaviours.Count; }
        }

        public void RemoveBehaviour( BehaviourNode behaviour )
        {
            var actionBehaviourNode = behaviour as ActionBehaviourNode;
            if ( actionBehaviourNode != null )
                _actionBehaviours.Remove( actionBehaviourNode.Guid );

            var compoundBehaviourNode = behaviour as CompoundBehaviourNode;
            if ( compoundBehaviourNode != null )
            {
                _compoundBehaviours.Remove( compoundBehaviourNode.Guid );
            }
        }

        public void AddBehaviour( BehaviourNode behaviour )
        {
            var actionBehaviourNode = behaviour as ActionBehaviourNode;
            if ( actionBehaviourNode != null )
                _actionBehaviours.Add( actionBehaviourNode.Guid, actionBehaviourNode );

            var compoundBehaviourNode = behaviour as CompoundBehaviourNode;
            if ( compoundBehaviourNode != null )
            {
                _compoundBehaviours.Add( compoundBehaviourNode.Guid, compoundBehaviourNode );
            }
        }

        public BehaviourNode GetBehaviourNode( string nodeGuid )
        {
            ActionBehaviourNode actionNode;
            if ( _actionBehaviours.TryGetValue( nodeGuid, out actionNode ) )
            {
                return actionNode;
            }

            CompoundBehaviourNode compoundNode;
            if ( _compoundBehaviours.TryGetValue( nodeGuid, out compoundNode ) )
            {
                return compoundNode;
            }
            
            throw new ArgumentException( "No node found with Guid " + nodeGuid );
        }

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
            RightCorner,
            FarthestCornerFromPlayer,
            OntoPlayer,
            LeftPlatform,
            RightPlatform
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
        private class ActionBehaviourDictionary : SerializableDictionary<string, ActionBehaviourNode>
        {
        }

        [ Serializable ]
        private class CompoundBehavioursDictionary : SerializableDictionary<string, CompoundBehaviourNode>
        {
        }
    }
}