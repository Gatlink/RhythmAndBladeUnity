using System;
using Gamelogic.Extensions;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class CameraController : GLMonoBehaviour
{
    private Mobile _target;

    [ ReadOnly ]
    public Vector2 Offset;

    [ FormerlySerializedAs( "TrackingInertia" ) ]
    public Vector2 MinTrackingInertia;

    public float MinTrackingDistance = 0;

    public Vector2 MaxTrackingInertia;

    public float MaxTrackingDistance = 5;

    [ ClampedCurve ]
    public AnimationCurve TrackingInertiaCurve = AnimationCurve.Linear( 0, 0, 1, 1 );

    [ FormerlySerializedAs( "OffsetInertia" ) ]
    public float DirectionChangeInertia;

    public float ZoomInertia;

    private Vector2 _currentVelocity;

    private Vector2 _currentOffset;

    private Vector2 _currentOffsetVelocity;

    private Rail _currentConstraint;

    private LayerMask _cameraConstraintsLayer;
    private CameraZoomConstraint _cameraZoomConstraint;

    private float _baseCameraZoom;
    private float _currentZoomVelocity;
    private float _currentZoom;
    private Camera _camera;

    public void Start()
    {
        _target = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Mobile>();
        _cameraConstraintsLayer = 1 << LayerMask.NameToLayer( Layers.CameraConstraint );
        _camera = GetComponentInChildren<Camera>();
        _baseCameraZoom = _camera.orthographicSize;
        _currentOffset = Offset;
        _currentZoom = 1;
    }

    public void LateUpdate()
    {
        CheckConstraints();

        var targetOffset = Offset;
        targetOffset.x *= _target.Direction;

        _currentOffset = Vector2.SmoothDamp( _currentOffset, targetOffset, ref _currentOffsetVelocity,
            DirectionChangeInertia,
            Single.MaxValue, Time.deltaTime );

        var desiredPosition = (Vector2) _target.transform.position + _currentOffset;
        var targetZoom = 1f;

        if ( _currentConstraint != null )
        {
            desiredPosition = _currentConstraint.GetNearestPoint( desiredPosition );
            if ( _cameraZoomConstraint != null )
            {
                targetZoom =
                    _cameraZoomConstraint.GetZoom( _currentConstraint.EvaluateNormalizedPosition( desiredPosition ) );

            }
        }

        var pos = transform.position;
        var inertia = GetTrackingInertia( Vector2.Distance( pos, desiredPosition ) );
        pos.x = Mathf.SmoothDamp( pos.x, desiredPosition.x, ref _currentVelocity.x, inertia.x );
        pos.y = Mathf.SmoothDamp( pos.y, desiredPosition.y, ref _currentVelocity.y, inertia.y );

        _currentZoom = Mathf.SmoothDamp( _currentZoom, targetZoom, ref _currentZoomVelocity, ZoomInertia );
        _camera.orthographicSize = _baseCameraZoom * _currentZoom;

        transform.position = pos;
    }

    public Vector2 GetTrackingInertia( float distance )
    {
        var t = TrackingInertiaCurve.Evaluate( MathUtils.InvLerp( MinTrackingDistance, MaxTrackingDistance,
            distance ) );
        return Vector2.Lerp( MinTrackingInertia, MaxTrackingInertia, t );
    }

    private void CheckConstraints()
    {
        var constraint = Physics2D.OverlapPoint( _target.transform.position, _cameraConstraintsLayer );
        _currentConstraint = constraint != null ? constraint.GetComponentInParent<Rail>() : null;
        _cameraZoomConstraint =
            _currentConstraint != null ? _currentConstraint.GetComponent<CameraZoomConstraint>() : null;
    }

    [ InspectorButton ]
    public void SetPlayerOffset()
    {
        var player = GameObject.FindGameObjectWithTag( Tags.Player ).transform;
        Offset = transform.position - player.position;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if ( _target == null ) return;
        var targetOffset = Offset;
        targetOffset.x *= _target.Direction;

        Gizmos.color = Color.cyan;
        var from = (Vector2) _target.transform.position + targetOffset;
        var to = (Vector2) transform.position;
        Gizmos.DrawLine( from, to );
        using ( new Handles.DrawingScope( Color.cyan ) )
        {
            var style = new GUIStyle( GUI.skin.label );
            style.normal.textColor = Color.cyan;
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label( 0.5f * ( from + to ) + Vector2.up * 0.5f,
                new GUIContent( Vector2.Distance( from, to ).ToString( "F1" ) ), style );
        }
    }
#endif
}