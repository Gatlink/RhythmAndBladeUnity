using System;
using Gamelogic.Extensions;

namespace Controllers
{
    [Serializable]
    public class CompoundBehaviourNode : BehaviourNode
    {
        public OptionalInt HealthEndCondition;
        public bool Randomize;
        public bool LoopRepeat;        

        public NodeList ChildNodes = new NodeList();
    }
    
    [ Serializable ]
    public class NodeList : InspectorList<BehaviourNode>
    {
    }
}