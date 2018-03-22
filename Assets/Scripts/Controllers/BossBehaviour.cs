using System;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{
    [ CreateAssetMenu ]    
    public class BossBehaviour : ScriptableObject
    {
        public List<BehaviourNode> Behaviours = new List<BehaviourNode>();
        public BehaviourNode MainBehaviour;

        public void RemoveBehaviour( BehaviourNode behaviour )
        {
            Behaviours.Remove( behaviour );
        }

        public void AddBehaviour( BehaviourNode behaviour )
        {
            Behaviours.Add( behaviour );
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

    }
}