using System;
using UnityEngine;

namespace Controllers
{
    [ Serializable ]
    public class BehaviourNode
    {
        public string Name;

        [ HideInInspector ]
        public string Guid;

        public BehaviourNode( string name ) : this( name, System.Guid.NewGuid().ToString() )
        {
        }

        public BehaviourNode( string name, string id )
        {
            Name = name;
            Guid = id;
        }

        public static ActionBehaviourNode CreateActionBehaviourNode()
        {
            return new ActionBehaviourNode( "New Action Node" );
        }

        public static CompoundBehaviourNode CreateCompoundBehaviourNode()
        {
            return new CompoundBehaviourNode( "New Compound Node" );
        }
    }
}