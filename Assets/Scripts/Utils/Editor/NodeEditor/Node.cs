using System;
using System.Collections.Generic;
using Controllers;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    public class Node
    {
        private readonly string _title;
        public Rect Rect;
        private const int defaultWidth = 120;
        private const int defaultHeight = 60;
        private bool _isDragged;
        private bool _isSelected;

        public readonly ConnectionPoint InPoint;
        public readonly List<ConnectionPoint> OutPoints;

        private GUIStyle _style;
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

        private readonly GUIStyle _defaultNodeStyle;
        private readonly GUIStyle _selectedNodeStyle;

        private readonly Action<Node> _onRemoveNode;

        public readonly BossControllerBase Controller;

        public Node( BossControllerBase controller, Vector2 position, GUIStyle nodeStyle,
            GUIStyle selectedStyle,
            GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> onClickInPoint,
            Action<ConnectionPoint> onClickOutPoint, Action<Node> onClickRemoveNode )
        {
            Controller = controller;
            _title = controller.Name;
            Rect = new Rect( position.x - defaultWidth / 2f, position.y - defaultHeight / 2f, defaultWidth,
                defaultHeight );
            _style = nodeStyle;
            InPoint = new ConnectionPoint( this, ConnectionPointType.In, inPointStyle, onClickInPoint );
            var manager = Controller as BossControllerManager;
            if ( manager != null )
            {
                OutPoints = new List<ConnectionPoint>();
                for ( var i = 0; i < manager.SubControllers.Count ; i++ )
                {
                    OutPoints.Add( new ConnectionPoint( this, ConnectionPointType.Out, outPointStyle, onClickOutPoint ) );                    
                }                    
            }

            _defaultNodeStyle = nodeStyle;
            _selectedNodeStyle = selectedStyle;
            _onRemoveNode = onClickRemoveNode;
        }

        public void Drag( Vector2 delta )
        {
            Rect.position += delta;
        }

        public void Draw()
        {
            InPoint.Draw();
            if ( OutPoints != null )
            {
                foreach ( var outPoint in OutPoints )
                {
                    outPoint.Draw();
                }
            }

            using ( new GUI.GroupScope( Rect, _style ) )
            {
                var rect = new Rect( Rect );
                rect.x = 10;
                rect.width -= 20;
                rect.y = 10;
                rect.height -= 20;
                GUI.Label( rect, _title, TitleLabelStyle );

//                if ( Controller is FixedScriptBossController )
//                {
//                    rect.y += 16;
//                    using ( new GUI.GroupScope( rect ) )
//                    {
//                        var editor = Editor.CreateEditor( Controller );
//                        editor.DrawDefaultInspector();
//                    }
//                }
            }

            //GUI.Box( Rect, _title, _style );
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
                                InspectorPopup.ShowInspectorPopup( Controller );
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
//            var genericMenu = new GenericMenu();
//            genericMenu.AddItem( new GUIContent( "Remove node" ), false, OnClickRemoveNode );
//            genericMenu.ShowAsContext();
        }

        private void OnClickRemoveNode()
        {
            if ( _onRemoveNode != null )
            {
                _onRemoveNode( this );
            }

//            Object.Destroy( Controller );
        }
    }
}