using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Gamelogic.Extensions;
using UnityEngine.Assertions;

namespace NodeEditor
{
    public class NodeBasedEditor : EditorWindow
    {
        private BossBehaviour _target;

        private Dictionary<string, Node> _nodes = new Dictionary<string, Node>();
        private int _nodeCount;

        public GUIStyle ActionNodeStyle;
        public GUIStyle SelectedActionNodeStyle;

        public GUIStyle CompoundNodeStyle;
        public GUIStyle SelectedCompoundNodeStyle;

        public GUIStyle InPointStyle;
        public GUIStyle OutPointStyle;

        public GUIStyle TitleLabelStyle;
        public GUIStyle ActionNodeContentStyle;

        private PositionCacheStore _cacheStore;

        private ConnectionPoint _selectedInPoint;
        private ConnectionPoint _selectedOutPoint;

        private Vector2 _offset;
        private Vector2 _drag;
        private Color _connectionColor;
        private InspectorPopup _inspectorPopup;
        private PositionCache _nodePositionCache;

        public static void EditBossBehaviour( BossBehaviour target )
        {
            var window = GetWindow<NodeBasedEditor>();
            window.titleContent = new GUIContent( target.name );
        }

        private void OnEnable()
        {
            ActionNodeStyle = new GUIStyle();
            ActionNodeStyle.normal.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/node1.png" ) as Texture2D;
            ActionNodeStyle.border = new RectOffset( 12, 12, 12, 12 );

            SelectedActionNodeStyle = new GUIStyle();
            SelectedActionNodeStyle.normal.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/node1 on.png" ) as Texture2D;
            SelectedActionNodeStyle.border = new RectOffset( 12, 12, 12, 12 );

            CompoundNodeStyle = new GUIStyle();
            CompoundNodeStyle.normal.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/node2.png" ) as Texture2D;
            CompoundNodeStyle.border = new RectOffset( 12, 12, 12, 12 );

            SelectedCompoundNodeStyle = new GUIStyle();
            SelectedCompoundNodeStyle.normal.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/node2 on.png" ) as Texture2D;
            SelectedCompoundNodeStyle.border = new RectOffset( 12, 12, 12, 12 );

            InPointStyle = new GUIStyle();
            InPointStyle.normal.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/node1 hex.png" ) as Texture2D;
//            _inPointStyle.active.background =
//                EditorGUIUtility.Load( "builtin skins/lightskin/images/btn act.png" ) as Texture2D;
            InPointStyle.border = new RectOffset( 1, 1, 1, 1 );

            OutPointStyle = new GUIStyle();
            OutPointStyle.normal.background =
                EditorGUIUtility.Load( "builtin skins/lightskin/images/node1 hex.png" ) as Texture2D;
//            _outPointStyle.active.background =
//                EditorGUIUtility.Load( "builtin skins/lightskin/images/btn act.png" ) as Texture2D;
            OutPointStyle.border = new RectOffset( 2, 2, 2, 2 );


            TitleLabelStyle = new GUIStyle();
            TitleLabelStyle.alignment = TextAnchor.UpperCenter;

            ActionNodeContentStyle = new GUIStyle();
            ActionNodeContentStyle.alignment = TextAnchor.MiddleCenter;

            _connectionColor = Color.black;

            autoRepaintOnSceneChange = true;

            const string cacheName = "NodeEditorCache";
            _cacheStore = EditorGUIUtility.Load( cacheName + ".asset" ) as PositionCacheStore;
            if ( _cacheStore == null )
            {
                _cacheStore = CreateInstance<PositionCacheStore>();
                _cacheStore.name = cacheName;
                AssetDatabase.CreateAsset( _cacheStore,
                    "Assets/Editor Default Resources/" + cacheName + ".asset" );
                AssetDatabase.SaveAssets();
            }

            Selection.selectionChanged += UpdateTarget;
        }

        private void OnDisable()
        {
            AssetDatabase.SaveAssets();
            Selection.selectionChanged -= UpdateTarget;
        }

        private void UpdateTarget()
        {
            var selection = Selection.activeObject as BossBehaviour;
            if ( selection == null )
            {
                titleContent = new GUIContent( "Boss Behaviour" );
                _target = null;
                _nodePositionCache = null;
                ClearGraph();
                return;
            }

            if ( selection != _target )
            {
                _target = selection;
                _nodePositionCache = _cacheStore.GetCache( _target.name );
                titleContent = new GUIContent( selection.name );
                PopulateGraph();
            }
        }

        private void PopulateGraph()
        {
            var loadFromCache =
                _nodePositionCache.ValidateCache( _target.GetAllBehaviourNodes()
                    .Select( behaviour => behaviour.Guid ) );

            ClearGraph();
            var actionBehaviourNodes = _target.GetActionBehaviourNodes().ToArray();

            var stepX = Mathf.Max( 100, position.width / actionBehaviourNodes.Length );
            var nodePosition = new Vector2( stepX / 2, position.height - 100 );

            foreach ( var actionBehaviourNode in actionBehaviourNodes )
            {
                if ( loadFromCache )
                {
                    nodePosition = _nodePositionCache.GetCachedPosition( actionBehaviourNode.Guid );
                }

                CreateNode( nodePosition, actionBehaviourNode );
                nodePosition.x += stepX;
            }

            var compoundBehaviourNodes = _target.GetCompoundBehaviourNodes().ToArray();
            stepX = Mathf.Max( 100, position.width / compoundBehaviourNodes.Length );
            nodePosition = new Vector2( stepX / 2, nodePosition.y - 100 );
            foreach ( var compoundBehaviourNode in compoundBehaviourNodes )
            {
                if ( loadFromCache )
                {
                    nodePosition = _nodePositionCache.GetCachedPosition( compoundBehaviourNode.Guid );
                }

                CreateCompoundNode( nodePosition, compoundBehaviourNode );
                nodePosition.x += stepX;
            }

            foreach ( var compoundBehaviourNode in compoundBehaviourNodes )
            {
                var topNode = (CompoundNode) _nodes[ compoundBehaviourNode.Guid ];

                foreach ( var childNodeGuid in compoundBehaviourNode.ChildNodes )
                {
                    topNode.AddOutConnectionPoint();

                    if ( !loadFromCache )
                    {
                        var bottomNode = _nodes[ childNodeGuid ];
                        if ( bottomNode is CompoundNode )
                        {
                            topNode.Rect.y = Mathf.Min( bottomNode.Rect.y - 100, topNode.Rect.y );
                            UpdateCacheNodePosition( topNode );
                        }
                    }
                }
            }

            Repaint();
        }

        private void OnGUI()
        {
            if ( _target == null )
            {
                UpdateTarget();
            }

            if ( _nodes.Count == 0 && _nodeCount != 0 )
            {
                PopulateGraph();
            }
            else if ( _nodes.Count != _nodeCount )
            {
                Debug.LogError( "nodeCount lost sync : " + ( _nodes.Count - _nodeCount ) );
                _nodeCount = _nodes.Count;
            }

            DrawGrid( 20, 0.2f, Color.gray );
            DrawGrid( 100, 0.4f, Color.gray );

            DrawNodes();
            DrawConnections();

            DrawConnectionLine( Event.current );

            var size = new Vector2( 100, 20 );
            const int spacing = 5;
            const int buttonCount = 2;
            using ( new GUI.GroupScope( new Rect( 5, 5, size.x, 2 * size.y + (buttonCount - 1) * spacing ) ) )
            {
                var rect = new Rect( Vector2.zero, size );
                if ( _target != null && GUI.Button( rect, "Reload" ) )
                {
                    PopulateGraph();
                }

                rect.y += size.y + spacing;
                if ( _target != null && GUI.Button( rect, "Reset Layout" ) )
                {
                    _nodePositionCache.InvalidateCache();
                    PopulateGraph();
                }
            }

            ProcessNodeEvents( Event.current );
            ProcessEvents( Event.current );

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
            using ( new Handles.DrawingScope( gridColor.WithAlpha( gridOpacity ) ) )
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
            if ( Event.current.type != EventType.Repaint ) return;

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
            if ( Event.current.type != EventType.Repaint ) return;

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

        public void OnClickInPoint( ConnectionPoint inPoint )
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

        public void OnClickOutPoint( ConnectionPoint outPoint )
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

        public bool OnClickNode( Node node )
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

        public void OnClickRemoveNode( Node node )
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
            _nodeCount--;
            _target.RemoveBehaviour( node.BehaviourNode );
        }

        private void ClearGraph()
        {
            _nodes.Clear();
            _nodeCount = 0;
            if ( _inspectorPopup != null )
            {
                _inspectorPopup.Close();
                _inspectorPopup = null;
            }

            Repaint();
        }

        private Node CreateCompoundNode( Vector2 mousePosition, CompoundBehaviourNode compoundNode )
        {
            var node = new CompoundNode( this, compoundNode, mousePosition );
            UpdateCacheNodePosition( node );

            _nodes.Add( compoundNode.Guid, node );
            _nodeCount++;
            return node;
        }

        private Node CreateNode( Vector2 mousePosition, ActionBehaviourNode actionNode )
        {
            var node = new Node( this, actionNode, mousePosition );
            UpdateCacheNodePosition( node );

            _nodes.Add( actionNode.Guid, node );
            _nodeCount++;
            return node;
        }

        public void OnClickMoveConnectionPoint( ConnectionPoint point, int direction )
        {
            var node = (CompoundNode) point.Node;
            var index = node.OutPoints.IndexOf( point );
            Assert.IsFalse( index == 0 && direction == -1 || index == node.OutPoints.Count - 1 && direction == 1 );
            Swap( node.OutPoints, index, index + direction );
            Swap( node.BehaviourNode.ChildNodes, index, index + direction );
        }

        private void Swap( IList list, int i1, int i2 )
        {
            var tmp = list[ i1 ];
            list[ i1 ] = list[ i2 ];
            list[ i2 ] = tmp;
        }

        public void OnClickMainNode( Node node )
        {
            _target.MainBehaviour = node.BehaviourNode;
        }

        public bool IsMainNode( Node node )
        {
            return _target.MainBehaviourGuid == node.BehaviourNode.Guid;
        }

        public void OnClickRemoveConnectionPoint( ConnectionPoint connectionPoint )
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

        public void OnDoubleClickNode( Node node )
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

        public void UpdateCacheNodePosition( Node node )
        {
            _nodePositionCache.CachePosition( node.BehaviourNode.Guid, new Vector2( node.Rect.center.x, node.Rect.y ) );
        }
    }
}