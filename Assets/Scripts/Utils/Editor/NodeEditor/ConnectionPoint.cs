using System;
using UnityEngine;

namespace NodeEditor
{
    public enum ConnectionPointType
    {
        In,
        Out
    }

    public class ConnectionPoint
    {
        public Rect Rect;

        private readonly ConnectionPointType _type;

        public readonly Node Node;

        private readonly GUIStyle _style;

        private readonly Action<ConnectionPoint> _onClickConnectionPoint;

        public ConnectionPoint( Node node, ConnectionPointType type, GUIStyle style,
            Action<ConnectionPoint> onClickConnectionPoint )
        {
            Node = node;
            _type = type;
            _style = style;
            _onClickConnectionPoint = onClickConnectionPoint;
            Rect = new Rect( 0, 0, 20f, 10f );
        }

        public void Draw()
        {
            if ( _type == ConnectionPointType.In )
            {
                Rect.x = Node.Rect.x + Node.Rect.width * 0.5f - Rect.width * 0.5f;
                Rect.y = Node.Rect.y - Rect.height + 9;    
            }
            else
            {
                var i = Node.OutPoints.IndexOf( this );
                var step = Node.Rect.width / ( Node.OutPoints.Count + 1 );
                Rect.x = Node.Rect.x + (i + 1) * step - Rect.width * 0.5f;
                Rect.y = Node.Rect.y + Node.Rect.height - 10;
            }
            
            if ( GUI.Button( Rect, "", _style ) )
            {
//                if ( _onClickConnectionPoint != null )
//                {
//                    _onClickConnectionPoint( this );
//                }
            }
        }
    }
}