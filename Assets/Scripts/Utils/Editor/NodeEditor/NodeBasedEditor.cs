using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Controllers;

namespace NodeEditor
{
    public class NodeBasedEditor : EditorWindow
    {
        private GameObject _bossActor;

        private Dictionary<BossControllerBase, Node> _nodes;

        //private List<Node> _nodes;
        private List<Connection> _connections;

        private GUIStyle _nodeStyle;
        private GUIStyle _selectedNodeStyle;
        private GUIStyle _inPointStyle;
        private GUIStyle _outPointStyle;

        private ConnectionPoint _selectedInPoint;
        private ConnectionPoint _selectedOutPoint;

        private Vector2 _offset;
        private Vector2 _drag;
        private Color _connectionColor;

        [ MenuItem( "Window/Boss Controller Editor" ) ]
        private static void OpenWindow()
        {
            var window = GetWindow<NodeBasedEditor>();
            window.titleContent = new GUIContent( "Node Based Boss Controller Editor" );
        }

        private void OnEnable()
        {
            _nodeStyle = new GUIStyle();
            _nodeStyle.normal.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/node1.png" ) as Texture2D;
            _nodeStyle.border = new RectOffset( 12, 12, 12, 12 );

            _selectedNodeStyle = new GUIStyle();
            _selectedNodeStyle.normal.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/node1 on.png" ) as Texture2D;
            _selectedNodeStyle.border = new RectOffset( 12, 12, 12, 12 );

            _inPointStyle = new GUIStyle();
            _inPointStyle.normal.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/btn.png" ) as Texture2D;
            _inPointStyle.active.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/btn act.png" ) as Texture2D;
            _inPointStyle.border = new RectOffset( 2, 2, 2, 2 );

            _outPointStyle = new GUIStyle();
            _outPointStyle.normal.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/btn.png" ) as Texture2D;
            _outPointStyle.active.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/btn act.png" ) as Texture2D;
            _outPointStyle.border = new RectOffset( 2, 2, 2, 2 );

            _connectionColor = Color.black;

            _bossActor = GameObject.FindWithTag( Tags.Boss );
            if ( _bossActor == null )
            {
                Debug.LogError( "Boss Actor not found in current scene" );
            }
            else
            {
                PopulateGraph();
            }
        }

        private void PopulateGraph()
        {
            var fixedScriptControllers = _bossActor.GetComponents<FixedScriptBossController>();

            var stepX = Mathf.Max( 100, position.width / fixedScriptControllers.Length );
            var mousePosition = new Vector2( stepX / 2, position.height - 100 );
            foreach ( var controller in fixedScriptControllers )
            {
                CreateNode( mousePosition, controller );
                mousePosition.x += stepX;
            }

            var compoundControllers = _bossActor.GetComponents<BossControllerManager>();
            stepX = Mathf.Max( 100, position.width / compoundControllers.Length );
            mousePosition = new Vector2( stepX / 2, mousePosition.y - 100 );
            foreach ( var controller in compoundControllers )
            {
                CreateNode( mousePosition, controller );
                mousePosition.x += stepX;
            }

            foreach ( var compoundController in compoundControllers )
            {
                for ( var index = 0; index < compoundController.SubControllers.Count; index++ )
                {
                    var subController = compoundController.SubControllers[ index ];
// make connection
                    CreateConnection( _nodes[ subController ].InPoint,
                        _nodes[ compoundController ].OutPoints[ index ] );

                    if ( subController is BossControllerManager )
                    {
                        var topNode = _nodes[ compoundController ];
                        var bottomNode = _nodes[ subController ];
                        topNode.Rect.y = Mathf.Min( bottomNode.Rect.y - 100, topNode.Rect.y );
                    }
                }
            }

            GUI.changed = true;
        }

        private void OnGUI()
        {
            DrawGrid( 20, 0.2f, Color.gray );
            DrawGrid( 100, 0.4f, Color.gray );

            DrawNodes();
            DrawConnections();

            DrawConnectionLine( Event.current );

            ProcessNodeEvents( Event.current );
            ProcessEvents( Event.current );

            if ( GUI.Button( new Rect( 10, 10, 50, 20 ), "Reload" ) )
            {
                ClearGraph();
                PopulateGraph();
            }

            if ( GUI.changed ) Repaint();
        }

        private void DrawGrid( float gridSpacing, float gridOpacity, Color gridColor )
        {
            var widthDivs = Mathf.CeilToInt( position.width / gridSpacing ) + 1;
            var heightDivs = Mathf.CeilToInt( position.height / gridSpacing ) + 1;

            Handles.BeginGUI();
            using ( new Handles.DrawingScope( new Color( gridColor.r, gridColor.g, gridColor.b, gridOpacity ) ) )
            {
                _offset += _drag * 0.5f;
                var newOffset = new Vector3( _offset.x % gridSpacing, _offset.y % gridSpacing, 0 );

                for ( var i = 0; i <= widthDivs; i++ )
                {
                    Handles.DrawLine( new Vector3( gridSpacing * i, -gridSpacing, 0 ) + newOffset,
                        new Vector3( gridSpacing * i, position.height, 0f ) + newOffset );
                }

                for ( var j = 0; j <= heightDivs; j++ )
                {
                    Handles.DrawLine( new Vector3( -gridSpacing, gridSpacing * j, 0 ) + newOffset,
                        new Vector3( position.width, gridSpacing * j, 0f ) + newOffset );
                }
            }

            Handles.EndGUI();
        }

        private void DrawNodes()
        {
            if ( _nodes != null )
            {
                foreach ( var node in _nodes.Values )
                {
                    node.Draw();
                }
            }
        }

        private void DrawConnections()
        {
            if ( _connections != null )
            {
                foreach ( var connection in _connections )
                {
                    connection.Draw();
                }
            }
        }

        private void ProcessEvents( Event e )
        {
            _drag = Vector2.zero;

            switch ( e.type )
            {
                case EventType.MouseDown:
                    if ( e.button == 0 )
                    {
                        ClearConnectionSelection();
                    }

                    if ( e.button == 1 )
                    {
                        ProcessContextMenu( e.mousePosition );
                    }

                    break;

                case EventType.MouseDrag:
                    if ( e.button == 0 )
                    {
                        OnDrag( e.delta );
                    }

                    break;
            }
        }

        private void ProcessNodeEvents( Event e )
        {
            if ( _nodes != null )
            {
                foreach ( var node in _nodes.Values )
                {
                    var guiChanged = node.ProcessEvents( e );

                    if ( guiChanged )
                    {
                        GUI.changed = true;
                    }
                }
            }
        }

        private void DrawConnectionLine( Event e )
        {
            if ( _selectedInPoint != null && _selectedOutPoint == null )
            {
                Handles.DrawBezier(
                    _selectedInPoint.Rect.center,
                    e.mousePosition,
                    _selectedInPoint.Rect.center - Vector2.up * 50f,
                    e.mousePosition + Vector2.up * 50f,
                    _connectionColor,
                    null,
                    2f
                );

                GUI.changed = true;
            }

            if ( _selectedOutPoint != null && _selectedInPoint == null )
            {
                Handles.DrawBezier(
                    _selectedOutPoint.Rect.center,
                    e.mousePosition,
                    _selectedOutPoint.Rect.center + Vector2.up * 50f,
                    e.mousePosition - Vector2.up * 50f,
                    _connectionColor,
                    null,
                    2f
                );

                GUI.changed = true;
            }
        }

        private void ProcessContextMenu( Vector2 mousePosition )
        {
//            var genericMenu = new GenericMenu();
//            genericMenu.AddItem( new GUIContent( "Add node" ), false, () => OnClickAddNode( mousePosition ) );
//            genericMenu.ShowAsContext();
        }

        private void OnDrag( Vector2 delta )
        {
            _drag = delta;

            if ( _nodes != null )
            {
                foreach ( var node in _nodes.Values )
                {
                    node.Drag( delta );
                }
            }

            GUI.changed = true;
        }

        private void OnClickAddNode( Vector2 mousePosition )
        {
            // CreateNode( mousePosition, ... );
        }

        private void OnClickInPoint( ConnectionPoint inPoint )
        {
            _selectedInPoint = inPoint;

            if ( _selectedOutPoint != null )
            {
                if ( _selectedOutPoint.Node != _selectedInPoint.Node )
                {
                    CreateConnection( _selectedInPoint, _selectedOutPoint );
                    ClearConnectionSelection();
                }
                else
                {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickOutPoint( ConnectionPoint outPoint )
        {
            _selectedOutPoint = outPoint;

            if ( _selectedInPoint != null )
            {
                if ( _selectedOutPoint.Node != _selectedInPoint.Node )
                {
                    CreateConnection( _selectedInPoint, _selectedOutPoint );
                    ClearConnectionSelection();
                }
                else
                {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickRemoveNode( Node node )
        {
            if ( _connections != null )
            {
                var connectionsToRemove = new List<Connection>();

                foreach ( var connection in _connections )
                {
                    if ( connection.InPoint == node.InPoint ||
                         ( node.OutPoints != null && node.OutPoints.Contains( connection.OutPoint ) ) )
                    {
                        connectionsToRemove.Add( connection );
                    }
                }

                foreach ( var connection in connectionsToRemove )
                {
                    _connections.Remove( connection );
                }
            }

            _nodes.Remove( node.Controller );
        }

        private void OnClickRemoveConnection( Connection connection )
        {
            _connections.Remove( connection );
        }

        private void ClearGraph()
        {
            if ( _nodes != null )
            {
                _nodes.Clear();
            }

            if ( _connections != null )
            {
                _connections.Clear();
            }
        }

        private Node CreateNode( Vector2 mousePosition, BossControllerBase controller )
        {
            if ( _nodes == null )
            {
                _nodes = new Dictionary<BossControllerBase, Node>();
            }

            var node = new Node( controller, mousePosition, _nodeStyle, _selectedNodeStyle, _inPointStyle,
                _outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode );
            _nodes.Add( controller, node );

            return node;
        }

        private void CreateConnection( ConnectionPoint inPoint, ConnectionPoint outPoint )
        {
            if ( _connections == null )
            {
                _connections = new List<Connection>();
            }

            _connections.Add( new Connection( inPoint, outPoint, OnClickRemoveConnection, _connectionColor ) );
        }

        private void ClearConnectionSelection()
        {
            _selectedInPoint = null;
            _selectedOutPoint = null;
        }
    }
}