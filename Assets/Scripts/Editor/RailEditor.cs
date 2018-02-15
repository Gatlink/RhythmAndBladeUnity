using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions.Editor.Internal;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[ CustomEditor( typeof( Rail ) ) ]
public class RailEditor : GLEditor<Rail>
{
    private const float GizmoRadius = 0.15f;
    private const int DottedLinesSpace = 3;

    private static readonly Color GUIColor = new Color( 0.9f, 0.9f, 0.9f );
    private static readonly Color HandleColor = new Color( 0.4f, 0.5f, 0.7f );
    private static readonly Color GroundColor = new Color( 0.5f, 0.8f, 1f );
    private static readonly Color WallColor = new Color( 0.7f, 0.7f, 0.4f );
    private static readonly Color HighSlopeColor = new Color( 0.8f, 0.2f, 0.2f );

    private static GUIStyle _style;

    private Rail _rail;

    private static readonly Dictionary<Rail, BoxBoundsHandle> MultiSelectionHandlesDictionary =
        new Dictionary<Rail, BoxBoundsHandle>();

    private BoxBoundsHandle _multiSelectionHandle;

    private readonly int MovePointHandleHint = "MovePointHandleHint".GetHashCode();
    private readonly int MultiSelectionHandleHint = "MultiSelectionHandleHint".GetHashCode();
    private readonly int NewPointHandleHint = "NewPointHandleHint".GetHashCode();

    private static GUIStyle Style
    {
        get
        {
            if ( _style == null )
            {
                _style = new GUIStyle( GUI.skin.label );
                _style.alignment = TextAnchor.UpperLeft;
            }

            return _style;
        }
    }

    private void OnEnable()
    {
        _rail = target as Rail;
        if ( !MultiSelectionHandlesDictionary.TryGetValue( _rail, out _multiSelectionHandle ) )
        {
            var bounds = _rail.Bounds;
            _multiSelectionHandle = new BoxBoundsHandle
            {
                center = bounds.center,
                axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y,
                size = bounds.size + Vector3.one,
                midpointHandleSizeFunction = v => HandleUtility.GetHandleSize( v ) * GizmoRadius / 2
            };
            MultiSelectionHandlesDictionary.Add( _rail, _multiSelectionHandle );
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DrawInspectorButtons(1);
    }

    private static void DrawNotice()
    {
        var boxStyle = new GUIStyle( GUI.skin.box );
        boxStyle.alignment = TextAnchor.UpperLeft;
        var labelStyle = new GUIStyle( GUI.skin.label );

        GUIContent content;
        switch ( Tools.current )
        {
            case Tool.Move:
                content = new GUIContent( "Mouse drag to move points" +
                                          "\nHold Shift to snap horizontally" +
                                          "\nHold Alt to snap vertically" +
                                          "\nHold Ctrl to add new point" +
                                          "\nRight mouse click to delete point" );
                break;
            case Tool.Rect:
                content = new GUIContent( "Mouse drag bottom left handle to move selection" +
                                          "\nHold Shift to snap horizontally" +
                                          "\nHold Alt to snap vertically" +
                                          "\nHold Ctrl to move enclosed rail points" );
                break;
            default:
                content = new GUIContent( "Select Move tool (W) to move individual points" +
                                          "\nSelect Rect tool (T) to move multiples points" );
                break;
        }

        var size = labelStyle.CalcSize( content );

        Handles.BeginGUI();
        GUI.Box(
            new Rect( 10 * Vector2.one, size + new Vector2( boxStyle.padding.horizontal, boxStyle.padding.vertical ) ),
            content, boxStyle );
        Handles.EndGUI();
    }

    private void OnSceneGUI()
    {
        if ( Tools.current == Tool.Rect )
        {
            DrawRail();
            DrawMultiSelectionHandle();
        }
        else if ( Tools.current == Tool.Move )
        {
            DrawRailEditable();
        }
        else
        {
            DrawRail();
        }

        DrawNotice();
    }

    [ DrawGizmo( GizmoType.NonSelected | GizmoType.Pickable | GizmoType.Selected ) ]
    private static void NonSelectedSceneView( Rail rail, GizmoType gizmoType )
    {
        if ( rail.gameObject != Selection.activeGameObject )
        {
            DrawRailPickable( rail );
        }
    }

    private void DrawRailEditable()
    {
        var currentEvent = Event.current;
        var root = _rail.transform.position;
        var points = _rail.Points;
        using ( new Handles.DrawingScope( HandleColor ) )
        {
            for ( var index = 0; index < points.Count; index++ )
            {
                var point = points[ index ];
                var worldPosition = root + point;
                var cid = GUIUtility.GetControlID( MovePointHandleHint, FocusType.Passive );
                EditorGUI.BeginChangeCheck();
                worldPosition = Handles.FreeMoveHandle( cid, worldPosition, Quaternion.identity,
                    GizmoRadius * HandleUtility.GetHandleSize( worldPosition ), Vector3.zero,
                    Handles.CylinderHandleCap );
                if ( EditorGUI.EndChangeCheck() )
                {
                    Undo.RecordObject( _rail, "Move point " + index );
                    // snap horizontally
                    if ( currentEvent.shift )
                    {
                        worldPosition = SnapHorizontally( _rail, index, worldPosition );
                    }

                    // snap vertically
                    if ( currentEvent.alt )
                    {
                        worldPosition = SnapVertically( _rail, index, worldPosition );
                    }

                    _rail.Points[ index ] = worldPosition - root;
                }
            }
        }

        DrawLines( _rail );

        var mouseRay = HandleUtility.GUIPointToWorldRay( currentEvent.mousePosition );

        // add new point
        if ( currentEvent.control && GUIUtility.hotControl == 0 )
        {
            if ( Event.current.type == EventType.Layout )
            {
                HandleUtility.AddDefaultControl( GUIUtility.GetControlID( NewPointHandleHint, FocusType.Passive ) );
            }

            Vector2 newPosition = mouseRay.origin;

            var nearestSegment = _rail.EnumerateSegments()
                .OrderBy( segment =>
                    HandleUtility.DistancePointToLineSegment( newPosition, segment.From, segment.To ) )
                .First();

            // snap horizontally
            if ( currentEvent.shift )
            {
                newPosition = SnapHorizontally( newPosition, nearestSegment.From, nearestSegment.To );
            }

            // snap vertically
            if ( currentEvent.alt )
            {
                newPosition = SnapVertically( newPosition, nearestSegment.From, nearestSegment.To );
            }

            int newIndex = nearestSegment.FromIndex + 1;
            // handles rail extremity case
            if ( !_rail.Closed &&
                 ( nearestSegment.FromIndex == 0 || nearestSegment.FromIndex + 1 == points.Count - 1 ) )
            {
                var distToSegment =
                    HandleUtility.DistancePointToLineSegment( newPosition, nearestSegment.From, nearestSegment.To );
                if ( nearestSegment.FromIndex == 0 &&
                     Vector2.Distance( newPosition, nearestSegment.From ) <= distToSegment )
                {
                    DrawLine( new Rail.Segment( newPosition, nearestSegment.From ), true );
                    newIndex = nearestSegment.FromIndex;
                }
                else if ( nearestSegment.FromIndex + 1 == points.Count - 1 &&
                          Vector2.Distance( newPosition, nearestSegment.To ) <= distToSegment )
                {
                    DrawLine( new Rail.Segment( nearestSegment.To, newPosition ), true );
                    newIndex = nearestSegment.FromIndex + 2;
                }
                else
                {
                    DrawLine( new Rail.Segment( nearestSegment.From, newPosition ), true );
                    DrawLine( new Rail.Segment( newPosition, nearestSegment.To ), true );
                }
            }
            else
            {
                DrawLine( new Rail.Segment( nearestSegment.From, newPosition ), true );
                DrawLine( new Rail.Segment( newPosition, nearestSegment.To ), true );
            }

            DrawJointGizmo( newPosition, HandleColor );

            if ( currentEvent.type == EventType.MouseUp && currentEvent.button == 0 )
            {
                Undo.RecordObject( _rail, "Add point at " + newIndex );
                points.Insert( newIndex, (Vector3) newPosition - root );
                currentEvent.Use();
            }

            HandleUtility.Repaint();
        }
        else if ( currentEvent.type == EventType.MouseDown && currentEvent.button == 1 ) // delete point
        {
            for ( var index = 0; index < points.Count; index++ )
            {
                var point = points[ index ] + root;
                var dist = GizmoRadius * HandleUtility.GetHandleSize( point );
                if ( Vector2.SqrMagnitude( mouseRay.origin - point ) < dist * dist )
                {
                    Undo.RecordObject( _rail, "Remove point at " + index );
                    points.RemoveAt( index );
                    currentEvent.Use();
                }
            }
        }
    }

    private void DrawMultiSelectionHandle()
    {
        var currentEvent = Event.current;
        var root = _rail.transform.position;
        var points = _rail.Points;

        // multiple selection
        _multiSelectionHandle.DrawHandle();

        var multiSelectionAnchor = _multiSelectionHandle.center - _multiSelectionHandle.size / 2;
        var bounds = new Bounds( _multiSelectionHandle.center, _multiSelectionHandle.size );
        var multiSelectionAnchorInitial = multiSelectionAnchor;

        EditorGUI.BeginChangeCheck();
        multiSelectionAnchor = Handles.FreeMoveHandle( MultiSelectionHandleHint, multiSelectionAnchor,
            Quaternion.identity, GizmoRadius * HandleUtility.GetHandleSize( multiSelectionAnchor ), Vector3.zero,
            Handles.DotHandleCap );
        if ( EditorGUI.EndChangeCheck() )
        {
            var delta = multiSelectionAnchor - multiSelectionAnchorInitial;

            if ( currentEvent.shift )
            {
                delta.y = 0;
            }

            if ( currentEvent.alt )
            {
                delta.x = 0;
            }

            // move selectionBounds
            _multiSelectionHandle.center += delta;

            if ( currentEvent.control )
            {
                // move points
                Undo.RecordObject( _rail, "Move multiple points " );
                for ( var index = 0; index < points.Count; index++ )
                {
                    if ( bounds.Contains( points[ index ] + root ) )
                    {
                        points[ index ] += delta;
                    }
                }
            }
        }
    }

    private Vector3 SnapVertically( Rail rail, int index, Vector3 position )
    {
        int prev;
        int next;
        GetNeighbours( rail, index, out prev, out next );

        position -= rail.transform.position;
        var points = rail.Points;

        return SnapVertically( position, points[ prev ], points[ next ] ) + rail.transform.position;
    }

    private Vector3 SnapVertically( Vector3 position, Vector3 prev, Vector3 next )
    {
        var distPrev = Mathf.Abs( position.x - prev.x );
        var distNext = Mathf.Abs( position.x - next.x );

        if ( distPrev < distNext )
        {
            position.x = prev.x;
        }
        else
        {
            position.x = next.x;
        }

        return position;
    }

    private Vector3 SnapHorizontally( Rail rail, int index, Vector3 position )
    {
        int prev;
        int next;
        GetNeighbours( rail, index, out prev, out next );

        position -= rail.transform.position;
        var points = rail.Points;

        return SnapHorizontally( position, points[ prev ], points[ next ] ) + rail.transform.position;
    }

    private Vector3 SnapHorizontally( Vector3 position, Vector3 prev, Vector3 next )
    {
        var distPrev = Mathf.Abs( position.y - prev.y );
        var distNext = Mathf.Abs( position.y - next.y );

        if ( distPrev < distNext )
        {
            position.y = prev.y;
        }
        else
        {
            position.y = next.y;
        }

        return position;
    }

    private static void GetNeighbours( Rail rail, int index, out int prev, out int next )
    {
        var points = rail.Points;
        if ( index == 0 )
        {
            prev = rail.Closed ? points.Count - 1 : index;
        }
        else
        {
            prev = index - 1;
        }

        if ( index == points.Count - 1 )
        {
            next = rail.Closed ? 0 : index;
        }
        else
        {
            next = index + 1;
        }
    }

    private static void DrawRailPickable( Rail rail )
    {
        DrawLines( rail );
        if ( Event.current.type == EventType.MouseUp && Event.current.button == 0 )
        {
            var mouseRay = HandleUtility.GUIPointToWorldRay( Event.current.mousePosition );

            var minDistance = rail.EnumerateSegments()
                .Min( segment =>
                    HandleUtility.DistancePointToLineSegment( mouseRay.origin, segment.From, segment.To ) );
            var segmentCenter = rail.EnumerateSegments().First().Center;
            if ( minDistance * minDistance < HandleUtility.GetHandleSize( segmentCenter ) * GizmoRadius )
            {
                EditorApplication.update -= _setSelected;
                EditorApplication.update += _setSelected;
                _desiredSelection = rail.gameObject;

                Event.current.Use();
            }
        }
    }

    private void DrawRail()
    {
        DrawLines( _rail );
        var root = _rail.transform.position;
        foreach ( var point in _rail.Points )
        {
            DrawJointGizmo( point + root, HandleColor, 0.5f );
        }
    }

    private static GameObject _desiredSelection;

    private static void _setSelected()
    {
        if ( _desiredSelection != null )
        {
            Selection.activeGameObject = _desiredSelection;
            _desiredSelection = null;
            // prevent unecessary null checks
            EditorApplication.update -= _setSelected;
        }
    }

    private static void DrawLines( Rail rail )
    {
        foreach ( var segment in rail.EnumerateSegments() )
        {
            DrawLine( segment );
        }
    }

    private static void DrawLine( Rail.Segment segment, bool dotted = false )
    {
        var color = GroundColor;
        if ( segment.IsWall() )
        {
            color = WallColor;
        }
        else if ( segment.Slope > Rail.SlopeLimit )
        {
            color = HighSlopeColor;
        }

        using ( new Handles.DrawingScope( color ) )
        {
            Style.normal.textColor = Handles.color;

            if ( dotted )
            {
                Handles.DrawDottedLine( segment.From, segment.To, DottedLinesSpace );
            }
            else
            {
                Handles.DrawLine( segment.From, segment.To );
            }

            var label = segment.Length.ToString( "F1" );
            if ( !segment.IsWall() )
            {
                label += " | " + segment.Slope.ToString( "F0" ) + "°";
            }

            var labelPos = segment.Center + segment.Normal * -0.6f;
            var labelSize = GUI.skin.label.CalcSize( new GUIContent( label ) );
            var adjustedPos = HandleUtility
                .GUIPointToWorldRay( HandleUtility.WorldToGUIPoint( labelPos ) - 0.5f * labelSize ).origin;
            Handles.Label( adjustedPos, label, Style );
        }
    }

    private static void DrawJointGizmo( Vector3 position, Color color, float size = 1 )
    {
        using ( new Handles.DrawingScope( color ) )
        {
            Handles.DrawSolidDisc( position, Vector3.forward,
                size * 0.5f * GizmoRadius * HandleUtility.GetHandleSize( position ) );
        }
    }
}