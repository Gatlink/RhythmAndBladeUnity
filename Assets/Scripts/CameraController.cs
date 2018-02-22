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

    public void Start()
    {
        _target = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Actor>();
    }

    public void LateUpdate()
    {
        var targetOffset = Offset;
        targetOffset.x *= _target.Direction;

        _currentOffset = Vector2.SmoothDamp( _currentOffset, targetOffset, ref _currentOffsetVelocity, OffsetInertia,
            Single.MaxValue, Time.deltaTime );

        var desiredPosition = (Vector2) _target.transform.position + _currentOffset;

        var pos = transform.position;
        pos.x = Mathf.SmoothDamp( pos.x, desiredPosition.x, ref _currentVelocity.x, TrackingInertia.x );
        pos.y = Mathf.SmoothDamp( pos.y, desiredPosition.y, ref _currentVelocity.y, TrackingInertia.y );

        transform.position = pos;
    }

    [ InspectorButton ]
    public void SetPlayerOffset()
    {
        var player = GameObject.FindGameObjectWithTag( Tags.Player ).transform;
        Offset = transform.position - player.position;
    }
}