using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Controllers;

namespace NodeEditor
{
    public class NodeBasedEditor : EditorWindow
    {
        private GameObject _bossActor;

        private GameObject BossActor
        {
            get
            {
                if ( _bossActor == null )
                {
                    _bossActor = GameObject.FindWithTag( Tags.Boss );
                    if ( _bossActor == null )
                    {
                        Debug.LogError( "Boss Actor not found in current scene" );
                    }
                }

                return _bossActor;
            }
        }

        private readonly Dictionary<BossControllerBase, Node> _nodes = new Dictionary<BossControllerBase, Node>();

        private GUIStyle _nodeStyle;
        private GUIStyle _selectedNodeStyle;
        private GUIStyle _inPointStyle;
        private GUIStyle _outPointStyle;

        private ConnectionPoint _selectedInPoint;
        private ConnectionPoint _selectedOutPoint;

        private Vector2 _offset;
        private Vector2 _drag;
        private Color _connectionColor;
        private InspectorPopup _inspectorPopup;

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

            PopulateGraph();

            autoRepaintOnSceneChange = true;
        }

        private void PopulateGraph()
        {
            var actor = BossActor;
            if ( actor == null ) return;

            var fixedScriptControllers = actor.GetComponents<FixedScriptBossController>();

            var stepX = Mathf.Max( 100, position.width / fixedScriptControllers.Length );
            var mousePosition = new Vector2( stepX / 2, position.height - 100 );
            foreach ( var controller in fixedScriptControllers )
            {
                CreateNode( mousePosition, controller );
                mousePosition.x += stepX;
            }

            var compoundControllers = actor.GetComponents<BossControllerManager>();
            stepX = Mathf.Max( 100, position.width / compoundControllers.Length );
            mousePosition = new Vector2( stepX / 2, mousePosition.y - 100 );
            foreach ( var controller in compoundControllers )
            {
                CreateNode( mousePosition, controller );
                mousePosition.x += stepX;
            }

            foreach ( var compoundController in compoundControllers )
            {
                foreach ( var subController in compoundController.SubControllers )
                {
                    var node = (CompoundNode) _nodes[ compoundController ];
                    node.AddOutConnectionPoint();

                    if ( subController != null )
                    {
                        if ( subController is BossControllerManager )
                        {
                            var topNode = _nodes[ compoundController ];
                            var bottomNode = _nodes[ subController ];
                            topNode.Rect.y = Mathf.Min( bottomNode.Rect.y - 100, topNode.Rect.y );
                        }
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
                if ( _inspectorPopup != null )
                {
                    _inspectorPopup.Close();
                }

                ClearGraph();
                PopulateGraph();
            }

            if ( GUI.changed )
            {
                Repaint();
                if ( _inspectorPopup != null )
                {
                    _inspectorPopup.Repaint();
                }
            }
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
            foreach ( var node in _nodes.Values )
            {
                node.Draw();
            }
        }

        private void DrawConnections()
        {
            foreach ( var node in _nodes.Values.OfType<CompoundNode>() )
            {
                for ( var index = 0; index < node.OutPoints.Count; index++ )
                {
                    var connectedController = node.Controller.SubControllers[ index ];
                    if ( connectedController != null )
                    {
                        var inPoint = _nodes[ connectedController ].InPoint;
                        var outPoint = node.OutPoints[ index ];
                        DrawConnection( inPoint, outPoint );
                    }
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
            foreach ( var node in _nodes.Values )
            {
                var guiChanged = node.ProcessEvents( e );

                if ( guiChanged )
                {
                    GUI.changed = true;
                }
            }
        }

        private void DrawConnectionLine( Event e )
        {
            if ( _selectedInPoint != null && _selectedOutPoint == null )
            {
                var fromPosition = _selectedInPoint.Rect.center;
                var toPosition = e.mousePosition;
                DrawConnection( fromPosition, toPosition );

                GUI.changed = true;
            }

            if ( _selectedOutPoint != null && _selectedInPoint == null )
            {
                var fromPosition = e.mousePosition;
                var toPosition = _selectedOutPoint.Rect.center;

                DrawConnection( fromPosition, toPosition );

                GUI.changed = true;
            }
        }

        private void DrawConnection( Vector2 fromPosition, Vector2 toPosition )
        {
            Handles.DrawBezier(
                fromPosition,
                toPosition,
                fromPosition - Vector2.up * 50f,
                toPosition + Vector2.up * 50f,
                _connectionColor,
                null,
                2f
            );
        }

        private void DrawConnection( ConnectionPoint fromPoint, ConnectionPoint toPoint )
        {
            DrawConnection( fromPoint.Rect.center, toPoint.Rect.center );
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

            foreach ( var node in _nodes.Values )
            {
                node.Drag( delta );
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
                    // clear previous connection to this out point
                    RemoveConnection( _selectedOutPoint );

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
                    // clear previous connection to this out point
                    RemoveConnection( _selectedOutPoint );

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
            var connectionsToRemove = new List<ConnectionPoint>();

            var controller = node.Controller;
            foreach ( var compoundNode in _nodes.Values.OfType<CompoundNode>() )
            {
                var manager = compoundNode.Controller;
                for ( int i = 0; i < manager.SubControllers.Count; i++ )
                {
                    if ( manager.SubControllers[ i ] == controller )
                    {
                        manager.SubControllers[ i ] = null;
                        connectionsToRemove.Add( compoundNode.OutPoints[ i ] );
                    }
                }
            }

            foreach ( var connectionPoint in connectionsToRemove )
            {
                OnClickRemoveConnectionPoint( connectionPoint );
            }

            _nodes.Remove( node.Controller );
            DestroyImmediate( node.Controller );
        }

        private void ClearGraph()
        {
            _nodes.Clear();
        }

        private Node CreateNode( Vector2 mousePosition, BossControllerManager controller )
        {
            var node = new CompoundNode( controller, mousePosition, _nodeStyle, _selectedNodeStyle, _inPointStyle,
                _outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnDoubleClickNode,
                OnClickRemoveConnectionPoint );

            _nodes.Add( node.Controller, node );
            return node;
        }

        private Node CreateNode( Vector2 mousePosition, BossControllerBase controller )
        {
            var node = new Node( controller, mousePosition, _nodeStyle, _selectedNodeStyle, _inPointStyle,
                OnClickInPoint, OnClickRemoveNode, OnDoubleClickNode );

            _nodes.Add( node.Controller, node );
            return node;
        }

        private void OnClickRemoveConnectionPoint( ConnectionPoint connectionPoint )
        {
            if ( connectionPoint.Type == ConnectionPointType.In )
            {
                Debug.LogError( "Cannot remove 'in' conection point !" );
                return;
            }

            var node = (CompoundNode) connectionPoint.Node;
            var index = node.OutPoints.IndexOf( connectionPoint );
            node.RemoveConnectionPoint( connectionPoint );
            node.Controller.SubControllers.RemoveAt( index );
        }

        private void OnDoubleClickNode( Node node )
        {
            _inspectorPopup = InspectorPopup.ShowInspectorPopup( node.Controller );
        }

        private void CreateConnection( ConnectionPoint inPoint, ConnectionPoint outPoint )
        {
            var inNode = inPoint.Node;
            var outNode = (CompoundNode) outPoint.Node;

            outNode.Controller.SubControllers[ outNode.OutPoints.IndexOf( outPoint ) ] = inNode.Controller;
        }

        private void ClearConnectionSelection()
        {
            _selectedInPoint = null;
            _selectedOutPoint = null;
        }

        private void RemoveConnection( ConnectionPoint outPoint )
        {
            var outNode = (CompoundNode) outPoint.Node;
            var index = outNode.OutPoints.IndexOf( outPoint );

            outNode.Controller.SubControllers[ index ] = null;
        }
    }
}