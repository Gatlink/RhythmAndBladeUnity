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
        public BehaviourNode BehaviourNode { get; protected set; }

        public Rect Rect;

        public readonly ConnectionPoint InPoint;

        private const int defaultWidth = 120;
        private const int defaultHeight = 60;
        private bool _isDragged;
        private bool _isSelected;
        
        protected readonly NodeBasedEditor Editor;

        private GUIStyle _style;

        protected virtual GUIStyle DefaultNodeStyle
        {
            get { return Editor.ActionNodeStyle; }
        }

        protected virtual GUIStyle SelectedNodeStyle
        {
            get { return Editor.SelectedActionNodeStyle; }
        }

        public Node( NodeBasedEditor editor, BehaviourNode behaviourNode, Vector2 position )
        {
            Editor = editor;
            BehaviourNode = behaviourNode;
            Rect = new Rect( position.x - defaultWidth / 2f, position.y, defaultWidth, defaultHeight );
            InPoint = new ConnectionPoint( this, ConnectionPointType.In, Editor.InPointStyle, Editor.OnClickInPoint,
                null, null );
        }

        public void Drag( Vector2 delta )
        {
            Rect.position += delta;
            Editor.UpdateCacheNodePosition( this );
        }

        public virtual void Draw()
        {
            if ( _style == null )
                _style = DefaultNodeStyle;

            InPoint.Draw();
            using ( new GUI.GroupScope( Rect, _style ) )
            {
                var rect = new Rect( Rect );
                rect.x = 10;
                rect.width -= 20;
                rect.y = 10;
                rect.height -= 20;
                GUI.Label( rect, BehaviourNode.Name, Editor.TitleLabelStyle );

                if ( Editor.IsMainNode( this ) )
                {
                    GUI.DrawTexture( new Rect( 12, 10, 16, 16 ), EditorGUIUtility.Load( "start.png" ) as Texture,
                        ScaleMode.ScaleToFit, true );
                }

                var actionBehaviourNode = BehaviourNode as ActionBehaviourNode;
                if ( actionBehaviourNode != null )
                {
                    var content = new GUIContent( string.Join( "\n",
                        actionBehaviourNode.Script.Select( action => action.Type.ToString() ).ToArray() ) );
                    var style = Editor.ActionNodeContentStyle;

                    Rect.height = defaultHeight + style.CalcHeight( content, rect.width ) - 2 * style.lineHeight;

                    rect.y += Editor.TitleLabelStyle.lineHeight;
                    rect.height -= Editor.TitleLabelStyle.lineHeight;

                    GUI.Label( rect, content, style );
                }
            }
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
                            if ( Editor.OnClickNode( this ) )
                            {
                                e.Use();
                            }
                            else
                            {
                                _isDragged = true;
                                GUI.changed = true;
                                _isSelected = true;
                                _style = SelectedNodeStyle;
                                if ( e.clickCount > 1 )
                                {
                                    Editor.OnDoubleClickNode( this );
                                    e.Use();
                                }
                            }
                        }
                        else
                        {
                            GUI.changed = true;
                            _isSelected = false;
                            _style = DefaultNodeStyle;
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
            genericMenu.AddItem( new GUIContent( "Remove node" ), false, () => Editor.OnClickRemoveNode( this ) );
            genericMenu.AddItem( new GUIContent( "Make node main node" ), false, () => Editor.OnClickMainNode( this ) );
            genericMenu.ShowAsContext();
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