﻿using System.Linq;
using UnityEditor;
using UnityEngine;

[ CustomEditor( typeof( Rail ) ), CanEditMultipleObjects ]
public class RailEditor : Editor
{
    private const float GizmoRadius = 0.15f;
    private const int DottedLinesSpace = 3;

    private static readonly Color HandleColor = new Color( 0.4f, 0.5f, 0.7f );
    private static readonly Color GroundColor = new Color( 0.5f, 0.8f, 1f );
    private static readonly Color WallColor = new Color( 0.7f, 0.5f, 0.4f );

    private static GUIStyle _style;


    private Rail _rail;

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
    }

    private void OnSceneGUI()
    {
        DrawRailEditable();
    }

    [ DrawGizmo( GizmoType.NonSelected | GizmoType.Pickable | GizmoType.Selected ) ]
    private static void NonSelectedSceneView( Rail rail, GizmoType gizmoType )
    {
        if ( rail.gameObject != Selection.activeGameObject )
        {
            DrawRail( rail );
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
                EditorGUI.BeginChangeCheck();
                worldPosition = Handles.FreeMoveHandle( worldPosition, Quaternion.identity,
                    GizmoRadius * HandleUtility.GetHandleSize( worldPosition ), Vector3.zero,
                    Handles.CylinderHandleCap );
                if ( EditorGUI.EndChangeCheck() )
                {
                    Undo.RecordObject( _rail, "Move point " + index );
                    // snap horizontally
                    if ( currentEvent.shift )
                    {
                        int prev;
                        int next;
                        GetNeighbours( _rail, index, out prev, out next );

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

        // add new point
        if ( currentEvent.control )
        {
            var cid = GUIUtility.GetControlID( FocusType.Passive );
            if ( Event.current.type == EventType.Layout )
                HandleUtility.AddDefaultControl( cid );

            var mouseRay = HandleUtility.GUIPointToWorldRay( currentEvent.mousePosition );
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

            DrawLine( new Rail.Segment( nearestSegment.From, newPosition ), true );
            DrawLine( new Rail.Segment( newPosition, nearestSegment.To ), true );

            DrawJointGizmo( newPosition );

            if ( currentEvent.type == EventType.MouseUp && currentEvent.button == 0 )
            {
                var newIndex = nearestSegment.FromIndex + 1;
                Undo.RecordObject( _rail, "Add point at " + newIndex );
                _rail.Points.Insert( newIndex, newPosition );
                currentEvent.Use();
            }

            HandleUtility.Repaint();
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

    private static void DrawRail( Rail rail )
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
        using ( new Handles.DrawingScope( segment.IsWall() ? WallColor : GroundColor ) )
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

    private static void DrawJointGizmo( Vector3 position )
    {
        Handles.DrawSolidDisc( position, Vector3.forward,
            0.5f * GizmoRadius * HandleUtility.GetHandleSize( position ) );
    }
}