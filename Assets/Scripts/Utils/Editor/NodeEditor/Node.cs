using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    public class Node
    {
        public BossControllerBase Controller { get; protected set; }

        public Rect Rect;

        public readonly ConnectionPoint InPoint;

        private const int defaultWidth = 120;
        private const int defaultHeight = 60;
        private bool _isDragged;
        private bool _isSelected;

        private readonly Action<Node> _onRemoveNode;
        private readonly Action<Node> _onDoubleClickNode;

        private GUIStyle _style;
        private readonly GUIStyle _defaultNodeStyle;
        private readonly GUIStyle _selectedNodeStyle;
        private static GUIStyle _titleLabelStyle;

        private static GUIStyle TitleLabelStyle
        {
            get
            {
                if ( _titleLabelStyle == null )
                {
                    _titleLabelStyle = new GUIStyle( "Label" );
                    _titleLabelStyle.alignment = TextAnchor.UpperCenter;
                }

                return _titleLabelStyle;
            }
        }

        public Node( BossControllerBase controller, Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle,
            GUIStyle inPointStyle, Action<ConnectionPoint> onClickInPoint, Action<Node> onClickRemoveNode, Action<Node> onDoubleClickNode )
        {
            Controller = controller;
            Rect = new Rect( position.x - defaultWidth / 2f, position.y - defaultHeight / 2f, defaultWidth,
                defaultHeight );
            _style = nodeStyle;
            InPoint = new ConnectionPoint( this, ConnectionPointType.In, inPointStyle, onClickInPoint, null );

            _defaultNodeStyle = nodeStyle;
            _selectedNodeStyle = selectedStyle;
            _onRemoveNode = onClickRemoveNode;
            _onDoubleClickNode = onDoubleClickNode;
        }

        public void Drag( Vector2 delta )
        {
            Rect.position += delta;
        }

        public virtual void Draw()
        {
            using ( new GUI.GroupScope( Rect, _style ) )
            {
                var rect = new Rect( Rect );
                rect.x = 10;
                rect.width -= 20;
                rect.y = 10;
                rect.height -= 20;
                GUI.Label( rect, Controller.Name, TitleLabelStyle );
            }

            InPoint.Draw();
        }

        public bool ProcessEvents( Event e )
        {
            switch ( e.type )
            {
                case EventType.MouseDown:
                    if ( e.button == 0 )
                    {
                        if ( Rect.Contains( e.mousePosition ) )
                        {
                            _isDragged = true;
                            GUI.changed = true;
                            _isSelected = true;
                            _style = _selectedNodeStyle;
                            if ( e.clickCount > 1 )
                            {
                                _onDoubleClickNode( this );
                                e.Use();
                            }
                        }
                        else
                        {
                            GUI.changed = true;
                            _isSelected = false;
                            _style = _defaultNodeStyle;
                        }
                    }

                    if ( e.button == 1 && _isSelected && Rect.Contains( e.mousePosition ) )
                    {
                        ProcessContextMenu();
                        e.Use();
                    }

                    break;

                case EventType.MouseUp:
                    _isDragged = false;
                    break;

                case EventType.MouseDrag:
                    if ( e.button == 0 && _isDragged )
                    {
                        Drag( e.delta );
                        e.Use();
                        return true;
                    }

                    break;
            }

            return false;
        }

        private void ProcessContextMenu()
        {
            var genericMenu = new GenericMenu();
            genericMenu.AddItem( new GUIContent( "Remove node" ), false, OnClickRemoveNode );
            genericMenu.ShowAsContext();
        }

        private void OnClickRemoveNode()
        {
            if ( _onRemoveNode != null )
            {
                _onRemoveNode( this );
            }
        }

        protected Vector2 GetInConnectionPointPosition()
        {
            var pos = Vector2.zero;
            pos.x = Rect.x + Rect.width * 0.5f;
            pos.y = Rect.y;
            return pos;
        }

        public virtual Vector2 GetConnectionPointPosition( ConnectionPoint point )
        {
            if ( point == InPoint )
            {
                return GetInConnectionPointPosition();
            }
            throw new ArgumentException( "Connection point not found" );
        }

        public virtual IEnumerable<ConnectionPoint> EnumerateConnectionPoints()
        {
            yield return InPoint;
        }
    }
}