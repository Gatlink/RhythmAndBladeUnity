using System;
using Gamelogic.Extensions;

namespace Controllers
{
    [ Serializable ]
    public class ActionBehaviourNode : BehaviourNode
    {
        public ActionList Script;
    }
    
    [ Serializable ]
    public class ActionList : InspectorList<BossBehaviour.Action>
    {
    }

}