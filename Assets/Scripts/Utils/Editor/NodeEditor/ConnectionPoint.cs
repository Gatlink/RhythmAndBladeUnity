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
        private readonly Action<ConnectionPoint, int> _onClickMoveConnectionPoint;

        public ConnectionPoint( Node node, ConnectionPointType type, GUIStyle style,
            Action<ConnectionPoint> onClickConnectionPoint, Action<ConnectionPoint> onClickRemoveConnectionPoint,
            Action<ConnectionPoint, int> onClickMoveConnectionPoint )
        {
            Node = node;
            Type = type;
            _style = style;
            _onClickConnectionPoint = onClickConnectionPoint;
            _onClickRemoveConnectionPoint = onClickRemoveConnectionPoint;
            _onClickMoveConnectionPoint = onClickMoveConnectionPoint;
            Rect = new Rect( 0, 0, 20f, 20f );
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
                if ( Type == ConnectionPointType.Out && Event.current.button == 1 )
                {
                    ProcessContextMenu();
                    Event.current.Use();
                }
                else if ( Event.current.button == 0 )
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
            var node = (CompoundNode) Node;
            var genericMenu = new GenericMenu();
            var index = node.OutPoints.IndexOf( this );
            var count = node.OutPoints.Count;

            genericMenu.AddItem( new GUIContent( "Remove connector" ), false,
                () => _onClickRemoveConnectionPoint( this ) );

            if ( index == 0 )
            {
                genericMenu.AddDisabledItem( new GUIContent( "Move left" ) );
            }
            else
            {
                genericMenu.AddItem( new GUIContent( "Move left" ), false,
                    () => _onClickMoveConnectionPoint( this, -1 ) );
            }

            if ( index == count - 1 )
            {
                genericMenu.AddDisabledItem( new GUIContent( "Move right" ) );
            }
            else
            {
                genericMenu.AddItem( new GUIContent( "Move right" ), false,
                    () => _onClickMoveConnectionPoint( this, 1 ) );
            }

            genericMenu.ShowAsContext();
        }
    }
}