using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{
    [ Serializable ]
    public class CompoundBehaviourNode : BehaviourNode
    {
        public bool Randomize;
        public bool LoopRepeat;

        // ReSharper disable once UnusedMember.Global
        public bool UseHealthEndCondition;

        [ ConditionalHide( "UseHealthEndCondition" ) ]
        public int HealthEndConditionLimit;

        [ SerializeField, HideInInspector ]
        private List<int> _childMultiplicators = new List<int>();

        private const int DefaultMultiplicator = 1;

        [ SerializeField, HideInInspector ]
        private List<string> _childNodes = new List<string>();

        public CompoundBehaviourNode( string name ) : base( name )
        {
        }

        public CompoundBehaviourNode( string name, string guid ) : base( name, guid )
        {
        }

        public void AddChild( string guid )
        {
            _childMultiplicators.Add( DefaultMultiplicator );
            _childNodes.Add( guid );
        }

        public void SetChild( int index, string guid )
        {
            _childNodes[ index ] = guid;
            _childMultiplicators[ index ] = DefaultMultiplicator;
        }

        public string GetChildAt( int index )
        {
            return _childNodes[ index ];
        }

        public void SwapChildren( int index1, int index2 )
        {
            Swap( _childNodes, index1, index2 );
            Swap( _childMultiplicators, index1, index2 );
        }

        public void RemoveChildAt( int index )
        {
            _childNodes.RemoveAt( index );
            _childMultiplicators.RemoveAt( index );
        }

        public IEnumerable<string> GetChildNodes()
        {
            return _childNodes.AsReadOnly();
        }

        public IEnumerable<int> GetChildMultiplicators()
        {
            return _childMultiplicators.AsReadOnly();
        }

        public int GetChildCount()
        {
            return _childNodes.Count;
        }

        private static void Swap( IList list, int i1, int i2 )
        {
            var tmp = list[ i1 ];
            list[ i1 ] = list[ i2 ];
            list[ i2 ] = tmp;
        }
    }
}