using System;
using System.Collections.Generic;
using Gamelogic.Extensions;
using UnityEngine;

namespace Controllers
{
    [ Serializable ]
    public class CompoundBehaviourNode : BehaviourNode
    {
        public OptionalInt HealthEndCondition;
        public bool Randomize;
        public bool LoopRepeat;

        public CompoundBehaviourNode( string name ) : base( name )
        {
        }

        public CompoundBehaviourNode( string name, string guid ) : base( name, guid )
        {
        }

        [ HideInInspector ]
        public List<string> ChildNodes = new List<string>();
    }

//    [ Serializable ]
//    public class NodeList : InspectorList<BehaviourNode>
//    {
//    }
}