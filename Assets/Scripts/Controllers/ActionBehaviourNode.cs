using System;
using System.Collections.Generic;
using Gamelogic.Extensions;

namespace Controllers
{
    [ Serializable ]
    public class ActionBehaviourNode : BehaviourNode
    {
//        public List<BossBehaviour.Action> Script;
        public ActionList Script;

        public ActionBehaviourNode( string name ) : base( name )
        {
        }

        public ActionBehaviourNode( string name, string guid ) : base( name, guid )
        {
        }
    }
    
    [ Serializable ]
    public class ActionList : InspectorList<BossBehaviour.Action>
    {
    }

}