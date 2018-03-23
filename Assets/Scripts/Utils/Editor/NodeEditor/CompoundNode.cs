using System;
using System.Collections.Generic;
using Controllers;
using UnityEngine;

namespace NodeEditor
{
    public class CompoundNode : Node
    {
        public new CompoundBehaviourNode BehaviourNode { get; protected set; }

        public readonly List<ConnectionPoint> OutPoints;

        private readonly GUIStyle _outPointStyle;
        private readonly Action<ConnectionPoint> _onClickOutPoint;
        private readonly Action<ConnectionPoint> _onClickRemoveConnectionPoint;

        public CompoundNode( CompoundBehaviourNode behaviourNode, Vector2 position, GUIStyle nodeStyle,
            GUIStyle selectedStyle, GUIStyle inPointStyle, GUIStyle outPointStyle, GUIStyle mainNodeStyle,
            Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint,
            Action<Node> onClickRemoveNode, Action<Node> onDoubleClickNode,
            Action<ConnectionPoint> onClickRemoveConnectionPoint, Func<Node, bool> onClickNode,
            Action<Node> onClickMainNode ) : base( behaviourNode, position, nodeStyle, selectedStyle, inPointStyle,
            mainNodeStyle, onClickInPoint, onClickRemoveNode, onDoubleClickNode, onClickNode, onClickMainNode )
        {
            BehaviourNode = behaviourNode;
            OutPoints = new List<ConnectionPoint>();
            _outPointStyle = outPointStyle;
            _onClickOutPoint = onClickOutPoint;
            _onClickRemoveConnectionPoint = onClickRemoveConnectionPoint;
        }

        public override void Draw()
        {
            base.Draw();
            if ( OutPoints != null )
            {
                foreach ( var outPoint in OutPoints )
                {
                    outPoint.Draw();
                }
            }
        }

        public Vector2 GetOutConnectionPointPosition( int indexInNode )
        {
            var pos = Vector2.zero;

            var step = Rect.width / ( OutPoints.Count + 1 );
            pos.x = Rect.x + ( indexInNode + 1 ) * step;
            pos.y = Rect.y + Rect.height;

            return pos;
        }

        public override Vector2 GetConnectionPointPosition( ConnectionPoint point )
        {
            if ( point == InPoint )
            {
                return GetInConnectionPointPosition();
            }

            var index = OutPoints.IndexOf( point );
            if ( index >= 0 )
            {
                return GetOutConnectionPointPosition( index );
            }

            throw new ArgumentException( "Connection point not found" );
        }

        public override IEnumerable<ConnectionPoint> EnumerateConnectionPoints()
        {
            yield return InPoint;
            foreach ( var outPoint in OutPoints )
            {
                yield return outPoint;
            }
        }

        public ConnectionPoint AddOutConnectionPoint()
        {
            var connectionPoint = new ConnectionPoint( this, ConnectionPointType.Out, _outPointStyle, _onClickOutPoint,
                _onClickRemoveConnectionPoint );
            OutPoints.Add( connectionPoint );
            return connectionPoint;
        }

        public void RemoveConnectionPoint( ConnectionPoint connectionPoint )
        {
            OutPoints.Remove( connectionPoint );
        }
    }
}