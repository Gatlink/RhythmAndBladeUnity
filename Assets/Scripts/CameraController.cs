using System;
using Gamelogic.Extensions;
using UnityEngine;

public class CameraController : GLMonoBehaviour
{
    private Actor _target;

    [ ReadOnly ]
    public Vector2 Offset;

    public Vector2 TrackingInertia = Vector2.zero;
    public float OffsetInertia;

    private Vector2 _currentVelocity;

    private Vector2 _currentOffset;
    private Vector2 _currentOffsetVelocity;

    private Rail _currentConstraint;
    private LayerMask _cameraConstraintsLayer;

    public void Start()
    {
        _target = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Actor>();
        _cameraConstraintsLayer = 1 << LayerMask.NameToLayer( Layers.CameraConstraint );
    }

    public void LateUpdate()
    {
        CheckConstraints();

        var targetOffset = Offset;
        targetOffset.x *= _target.Direction;

        _currentOffset = Vector2.SmoothDamp( _currentOffset, targetOffset, ref _currentOffsetVelocity, OffsetInertia,
            Single.MaxValue, Time.deltaTime );

        var desiredPosition = (Vector2) _target.transform.position + _currentOffset;

        if ( _currentConstraint != null )
        {
            desiredPosition = _currentConstraint.GetNearestPoint( desiredPosition );
        }

        var pos = transform.position;
        pos.x = Mathf.SmoothDamp( pos.x, desiredPosition.x, ref _currentVelocity.x, TrackingInertia.x );
        pos.y = Mathf.SmoothDamp( pos.y, desiredPosition.y, ref _currentVelocity.y, TrackingInertia.y );

        transform.position = pos;
    }

    private void CheckConstraints()
    {
        var constraint = Physics2D.OverlapPoint( _target.transform.position, _cameraConstraintsLayer );
        _currentConstraint = constraint != null ? constraint.GetComponent<Rail>() : null;
    }

    [ InspectorButton ]
    public void SetPlayerOffset()
    {
        var player = GameObject.FindGameObjectWithTag( Tags.Player ).transform;
        Offset = transform.position - player.position;
    }
}