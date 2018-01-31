﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ CustomEditor( typeof( Rail ) ) ]
public class RailEditor : Editor
{
    private const float GizmoRadius = 0.08f;
    private static readonly Color IdleColor = new Color( 0.4f, 0.4f, 0.5f );
    private static readonly Color ActiveColor = new Color( 0.4f, 0.5f, 0.7f );
    private static readonly Color SelectedColor = new Color( 0.5f, 0.8f, 1f );
    private static readonly Color WallColor = new Color( 0.7f, 0.5f, 0.4f );
    private static readonly Color NewColor = new Color( 0.5f, 0.8f, 1f, 0.5f );

    private const KeyCode AddKey = KeyCode.LeftControl;
    private const KeyCode DelKey = KeyCode.Delete;
    private const int DottedLinesSpace = 3;

    private static Vector3 _dragWorldStart;
    private static Vector2 _dragMouseCurrent;
    private static Vector2 _dragMouseStart;
    private static bool _addKeyDown;

    private Rail _rail;

    private void OnEnable()
    {
        _rail = target as Rail;
    }

    private void OnSceneGUI()
    {
        DrawHandlesEditable();
    }

    [DrawGizmo(GizmoType.NonSelected)]
    private static void NonSelectedSceneView( Rail rail, GizmoType gizmoType )
    {
        DrawHandles(rail);
    }
    
    private static void DrawHandles(Rail rail)
    {
        Handles.color = IdleColor;

        DrawLines(rail);

        foreach ( var point in rail.Points )
        {
            var cur = rail.transform.position + point;
            DrawJointGizmo( cur );
        }
    }

    private void DrawHandlesEditable()
    {
        Handles.color = ActiveColor;
        DrawLines(_rail);

        var newList = new List<Vector3>();
        var newIndex = -1;
        var newPoint = DisplayNewPoint( out newIndex );

        for ( var i = 0; i < _rail.Points.Count; ++i )
        {
            var cur = _rail.transform.position + _rail.Points[ i ];
            if ( DrawPointHandle( ref cur, i ) )
                newList.Add( cur - _rail.transform.position );
        }

        if ( newIndex != -1 )
        {
            newList.Insert( newIndex, newPoint - _rail.transform.position );
            Undo.RecordObject( _rail, "Add new point" );
        }

        _rail.Points.Clear();
        _rail.Points.AddRange( newList );
    }

    private static void DrawLines(Rail rail)
    {
        var oldColor = Handles.color;
        for ( var i = 0; i < rail.Points.Count - 1; ++i )
        {
            var cur = rail.transform.position + rail.Points[ i ];
            var next = rail.transform.position + rail.Points[ i + 1 ];

            if ( next.x == cur.x )
                Handles.color = WallColor;

            Handles.DrawLine( cur, next );
            Handles.color = oldColor;
        }
    }

    private bool ShouldBeAWall( int idx, Vector3 position )
    {
        if ( idx < 0 || idx >= _rail.Points.Count )
            return false;

        var point = _rail.Points[ idx ] + _rail.transform.position;
        var prevSlope = point.Slope( position );
        return prevSlope > 45;
    }

    private Vector3 DisplayNewPoint( out int newIndex )
    {
        newIndex = -1;

        var mousePosition = Event.current.mousePosition;
        mousePosition.y = Camera.current.pixelHeight - mousePosition.y;

        var position = Handles.inverseMatrix.MultiplyPoint( Camera.current.ScreenToWorldPoint( mousePosition ) );
        position.z = 0f;

        var prev = 0;
        var next = 0;
        if ( _addKeyDown )
        {
            _rail.GetPreviousAndNextPoint( position, out prev, out next );
            if ( ( prev == -1 || next == -1 ) && _rail.Points.Count > 1 )
                prev = next =
                    Vector2.Distance( _rail.Points[ 0 ] + _rail.transform.position, position ) >
                    Vector2.Distance( _rail.Points[ _rail.Points.Count - 1 ] + _rail.transform.position, position )
                        ? _rail.Points.Count - 1
                        : 0;

            if ( prev == -1 )
                prev = next = 0;

            if ( ShouldBeAWall( prev, position ) )
                position.x = _rail.Points[ prev ].x + _rail.transform.position.x;

            if ( ShouldBeAWall( next, position ) )
                position.x = _rail.Points[ next ].x + _rail.transform.position.x;
        }

        switch ( Event.current.type )
        {
            case EventType.KeyDown:
                if ( Event.current.keyCode == AddKey )
                    _addKeyDown = true;
                break;

            case EventType.KeyUp:
                if ( Event.current.keyCode == AddKey )
                    _addKeyDown = false;
                break;

            case EventType.Repaint:
                if ( _addKeyDown )
                {
                    var oldColor = Handles.color;
                    Handles.color = NewColor;

                    Handles.DrawDottedLine( _rail.Points[ prev ] + _rail.transform.position, position, DottedLinesSpace );
                    if ( next != -1 ) Handles.DrawDottedLine( _rail.Points[ next ] + _rail.transform.position, position, DottedLinesSpace );
                    DrawJointGizmo( position );

                    Handles.color = oldColor;
                    HandleUtility.Repaint();
                }
                break;

            case EventType.MouseDown:
                if ( _addKeyDown && Event.current.button == 0 )
                {
                    newIndex = next == 0 ? 0 : prev + 1;
                    Event.current.Use();
                    return position;
                }
                break;
        }

        return Vector3.zero;
    }

    private static void DrawJointGizmo( Vector3 position )
    {
        Handles.DrawSolidDisc( position, Vector3.forward, GizmoRadius * HandleUtility.GetHandleSize( position ) );
    }

    private bool DrawPointHandle( ref Vector3 position, int index )
    {
        var id = GUIUtility.GetControlID( FocusType.Passive );
        var screenPosition = Handles.matrix.MultiplyPoint( position );
        var cachedMatrix = Handles.matrix;

        switch ( Event.current.GetTypeForControl( id ) )
        {
            case EventType.MouseDown:
                if ( Event.current.button != 0 ) break;
                if ( HandleUtility.nearestControl == id )
                {
                    GUIUtility.hotControl = id;

                    _dragMouseCurrent = _dragMouseStart = Event.current.mousePosition;
                    _dragWorldStart = position;

                    Event.current.Use();
                    EditorGUIUtility.SetWantsMouseJumping( 1 );
                }
                break;

            case EventType.MouseUp:
                if ( Event.current.button != 0 ) break;
                if ( GUIUtility.hotControl == id && Event.current.button == 0 )
                {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                    EditorGUIUtility.SetWantsMouseJumping( 0 );
                }
                break;

            case EventType.MouseDrag:
                if ( GUIUtility.hotControl == id )
                {
                    _dragMouseCurrent += new Vector2( Event.current.delta.x, -Event.current.delta.y );

                    position = Camera.current.WorldToScreenPoint( Handles.matrix.MultiplyPoint( _dragWorldStart ) ) +
                               (Vector3) ( _dragMouseCurrent - _dragMouseStart );
                    position = Handles.matrix.inverse.MultiplyPoint( Camera.current.ScreenToWorldPoint( position ) );

                    if ( index > 0 && ShouldBeAWall( index - 1, position ) )
                        position.x = _rail.Points[ index - 1 ].x + _rail.transform.position.x;

                    if ( index < _rail.Points.Count - 1 && ShouldBeAWall( index + 1, position ) )
                        position.x = _rail.Points[ index + 1 ].x + _rail.transform.position.x;

                    GUI.changed = true;
                    Event.current.Use();
                    Undo.RecordObject( _rail, "Moved points" );
                }
                break;

            case EventType.KeyDown:
                if ( GUIUtility.hotControl == id && Event.current.keyCode == DelKey )
                {
                    Event.current.Use();
                    Undo.RecordObject( _rail, "Delete point" );
                    return false;
                }
                break;

            case EventType.Repaint:
                var currentColour = Handles.color;
                if ( id == GUIUtility.hotControl )
                    Handles.color = SelectedColor;

                Handles.matrix = Matrix4x4.identity;
                DrawJointGizmo( position );
                Handles.matrix = cachedMatrix;
                Handles.color = currentColour;
                break;

            case EventType.Layout:
                Handles.matrix = Matrix4x4.identity;
                HandleUtility.AddControl( id, HandleUtility.DistanceToCircle( screenPosition, GizmoRadius ) );
                Handles.matrix = cachedMatrix;
                break;
        }

        return true;
    }
}