using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Controllers;

namespace NodeEditor
{
    public class NodeBasedEditor : EditorWindow
    {
        private BossBehaviour _target;

        private readonly Dictionary<string, Node> _nodes = new Dictionary<string, Node>();

        private GUIStyle _nodeStyle;
        private GUIStyle _selectedNodeStyle;

        private GUIStyle _compoundNodeStyle;
        private GUIStyle _selectedCompoundNodeStyle;

        private GUIStyle _inPointStyle;
        private GUIStyle _outPointStyle;

        private ConnectionPoint _selectedInPoint;
        private ConnectionPoint _selectedOutPoint;

        private Vector2 _offset;
        private Vector2 _drag;
        private Color _connectionColor;
        private InspectorPopup _inspectorPopup;

        public static void EditBossBehaviour( BossBehaviour target )
        {
            var window = GetWindow<NodeBasedEditor>();
            window.titleContent = new GUIContent( "Node Based Boss Controller Editor" );
            window._target = target;
            window.PopulateGraph();
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

            _compoundNodeStyle = new GUIStyle();
            _compoundNodeStyle.normal.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/node1.png" ) as Texture2D;
            _compoundNodeStyle.border = new RectOffset( 12, 12, 12, 12 );

            _selectedCompoundNodeStyle = new GUIStyle();
            _selectedCompoundNodeStyle.normal.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/node1 on.png" ) as Texture2D;
            _selectedCompoundNodeStyle.border = new RectOffset( 12, 12, 12, 12 );


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

            autoRepaintOnSceneChange = true;

            Selection.selectionChanged += OnSelectionChangeHandler;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChangeHandler;
        }

        private void OnSelectionChangeHandler()
        {
            var selection = Selection.activeObject as BossBehaviour;
            if ( selection == null )
            {
                _target = null;
                ClearGraph();
                return;
            }

            if ( selection != _target )
            {
                _target = selection;
                PopulateGraph();
            }
        }

        private void PopulateGraph()
        {
            ClearGraph();
            var actionBehaviourNodes = _target.GetActionBehaviourNodes().ToArray();

            var stepX = Mathf.Max( 100, position.width / actionBehaviourNodes.Length );
            var mousePosition = new Vector2( stepX / 2, position.height - 100 );
            foreach ( var actionBehaviourNode in actionBehaviourNodes )
            {
                CreateNode( mousePosition, actionBehaviourNode );
                mousePosition.x += stepX;
            }

            var compoundBehaviourNodes = _target.GetCompoundBehaviourNodes().ToArray();
            stepX = Mathf.Max( 100, position.width / compoundBehaviourNodes.Length );
            mousePosition = new Vector2( stepX / 2, mousePosition.y - 100 );
            foreach ( var compoundBehaviourNode in compoundBehaviourNodes )
            {
                CreateCompoundNode( mousePosition, compoundBehaviourNode );
                mousePosition.x += stepX;
            }

            foreach ( var compoundBehaviourNode in compoundBehaviourNodes )
            {
                var topNode = (CompoundNode) _nodes[ compoundBehaviourNode.Guid ];

                foreach ( var childNodeGuid in compoundBehaviourNode.ChildNodes )
                {
                    topNode.AddOutConnectionPoint();

                    var bottomNode = _nodes[ childNodeGuid ];
                    if ( bottomNode is CompoundNode )
                    {
                        topNode.Rect.y = Mathf.Min( bottomNode.Rect.y - 100, topNode.Rect.y );
                    }
                }
            }

            Repaint();
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

            if ( _target != null && GUI.Button( new Rect( 10, 10, 50, 20 ), "Reload" ) )
            {
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
                    var connectedController = node.BehaviourNode.ChildNodes[ index ];
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
            var genericMenu = new GenericMenu();
            genericMenu.AddItem( new GUIContent( "Add Action Node" ), false, () => OnClickAddNode( mousePosition ) );
            genericMenu.AddItem( new GUIContent( "Add Compound Node" ), false,
                () => OnClickAddCoumpoudNode( mousePosition ) );
            genericMenu.ShowAsContext();
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

        private void OnClickAddCoumpoudNode( Vector2 mousePosition )
        {
            var behaviourNode = BehaviourNode.CreateCompoundBehaviourNode();
            _target.AddBehaviour( behaviourNode );
            CreateCompoundNode( mousePosition, behaviourNode );
        }

        private void OnClickAddNode( Vector2 mousePosition )
        {
            var behaviourNode = BehaviourNode.CreateActionBehaviourNode();
            _target.AddBehaviour( behaviourNode );
            CreateNode( mousePosition, behaviourNode );
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

        private bool OnClickNode( Node node )
        {
            if ( _selectedInPoint != null )
            {
                var compoundNode = node as CompoundNode;
                if ( compoundNode != null )
                {
                    if ( _selectedInPoint.Node != compoundNode )
                    {
                        _selectedOutPoint = compoundNode.AddOutConnectionPoint();
                        CreateConnection( _selectedInPoint, _selectedOutPoint );
                        ClearConnectionSelection();
                        return true;
                    }
                    else
                    {
                        ClearConnectionSelection();
                        return false;
                    }
                }
                else
                {
                    ClearConnectionSelection();
                    return false;
                }
            }
            else if ( _selectedOutPoint != null )
            {
                if ( _selectedOutPoint.Node != node )
                {
                    // clear previous connection to this out point
                    RemoveConnection( _selectedOutPoint );

                    _selectedInPoint = node.InPoint;
                    CreateConnection( _selectedInPoint, _selectedOutPoint );
                    ClearConnectionSelection();
                    return true;
                }
                else
                {
                    ClearConnectionSelection();
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void OnClickRemoveNode( Node node )
        {
            var connectionsToRemove = new List<ConnectionPoint>();

            var behaviourNodeGuid = node.BehaviourNode.Guid;
            foreach ( var compoundNode in _nodes.Values.OfType<CompoundNode>() )
            {
                var compound = compoundNode.BehaviourNode;
                for ( int i = 0; i < compound.ChildNodes.Count; i++ )
                {
                    if ( compound.ChildNodes[ i ] == behaviourNodeGuid )
                    {
                        compound.ChildNodes[ i ] = null;
                        connectionsToRemove.Add( compoundNode.OutPoints[ i ] );
                    }
                }
            }

            foreach ( var connectionPoint in connectionsToRemove )
            {
                OnClickRemoveConnectionPoint( connectionPoint );
            }

            _nodes.Remove( node.BehaviourNode.Guid );
            _target.RemoveBehaviour( node.BehaviourNode );
        }

        private void ClearGraph()
        {
            _nodes.Clear();
            if ( _inspectorPopup != null )
            {
                _inspectorPopup.Close();
                _inspectorPopup = null;
            }

            Repaint();
        }

        private Node CreateCompoundNode( Vector2 mousePosition, CompoundBehaviourNode compoundNode )
        {
            var node = new CompoundNode( compoundNode, mousePosition, _compoundNodeStyle, _selectedCompoundNodeStyle,
                _inPointStyle, _outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnDoubleClickNode,
                OnClickRemoveConnectionPoint, OnClickNode );

            _nodes.Add( compoundNode.Guid, node );
            return node;
        }

        private Node CreateNode( Vector2 mousePosition, ActionBehaviourNode actionNode )
        {
            var node = new Node( actionNode, mousePosition, _nodeStyle, _selectedNodeStyle, _inPointStyle,
                OnClickInPoint, OnClickRemoveNode, OnDoubleClickNode, OnClickNode );

            _nodes.Add( actionNode.Guid, node );
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
            node.BehaviourNode.ChildNodes.RemoveAt( index );
        }

        private void OnDoubleClickNode( Node node )
        {
            _inspectorPopup = InspectorPopup.ShowInspectorPopup( _target, node.BehaviourNode );
        }

        private void CreateConnection( ConnectionPoint inPoint, ConnectionPoint outPoint )
        {
            var inNode = inPoint.Node;
            var outNode = (CompoundNode) outPoint.Node;

            var index = outNode.OutPoints.IndexOf( outPoint );
            if ( index < outNode.BehaviourNode.ChildNodes.Count )
            {
                outNode.BehaviourNode.ChildNodes[ index ] = inNode.BehaviourNode.Guid;
            }
            else
            {
                outNode.BehaviourNode.ChildNodes.Add( inNode.BehaviourNode.Guid );
            }
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

            outNode.BehaviourNode.ChildNodes[ index ] = null;
        }
    }
}