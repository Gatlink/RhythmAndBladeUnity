using System;
using UnityEditor;
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

        public readonly ConnectionPointType Type;

        public readonly Node Node;

        private readonly GUIStyle _style;

        private readonly Action<ConnectionPoint> _onClickConnectionPoint;
        private readonly Action<ConnectionPoint> _onClickRemoveConnectionPoint;

        public ConnectionPoint( Node node, ConnectionPointType type, GUIStyle style,
            Action<ConnectionPoint> onClickConnectionPoint, Action<ConnectionPoint> onClickRemoveConnectionPoint )
        {
            Node = node;
            Type = type;
            _style = style;
            _onClickConnectionPoint = onClickConnectionPoint;
            _onClickRemoveConnectionPoint = onClickRemoveConnectionPoint;
            Rect = new Rect( 0, 0, 20f, 10f );
        }

        public void Draw()
        {
            Rect.position = Node.GetConnectionPointPosition( this );
            Rect.x -= Rect.width * 0.5f;

            if ( Type == ConnectionPointType.In )
            {
                Rect.y += -Rect.height + 9;
            }
            else
            {
                Rect.y += -10;
            }

            if ( GUI.Button( Rect, "", _style ) )
            {
                if ( _onClickRemoveConnectionPoint != null && Event.current.button == 1 )
                {
                    ProcessContextMenu();
                    Event.current.Use();
                }
                else
                {
                    if ( _onClickConnectionPoint != null )
                    {
                        _onClickConnectionPoint( this );
                    }
                }
            }
        }

        private void ProcessContextMenu()
        {
            var genericMenu = new GenericMenu();
            genericMenu.AddItem( new GUIContent( "Remove connector" ), false,
                () => _onClickRemoveConnectionPoint( this ) );
            genericMenu.ShowAsContext();
        }
    }
}