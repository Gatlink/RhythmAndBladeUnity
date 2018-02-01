﻿using UnityEngine;

public class StretchWithVelocity : MonoBehaviour
{
    public float MaxVelocity = 10;
    public float MaxSpeedStretch = 0.5f;
    public bool CompensateY = true;
    public float MinVelocity = 0.1f;

    private Vector2 _lastPosition;
    private Transform _childTransform;
    private Bounds _childBounds;

    private void Awake()
    {
        _childTransform = transform.GetChild( 0 );        
    }

    private void Start()
    {
        _childBounds = _childTransform.GetComponent<Renderer>().bounds;
        Debug.Log( _childBounds.size );
    }

    private void OnEnable()
    {
        _lastPosition = transform.position;
    }

    private void OnDisable()
    {
        ResetStretch();
    }

    private void LateUpdate()
    {
        if ( Time.deltaTime <= 0 ) return;

        var position = (Vector2) transform.position;
        var velocity = ( position - _lastPosition ) / Time.deltaTime;

        _lastPosition = position;

        if ( velocity.sqrMagnitude >= MinVelocity )
        {
            ApplyStretch( velocity, 1 + Mathf.Lerp( 0, MaxSpeedStretch, velocity.magnitude / MaxVelocity ) );
        }
    }

    private void ResetStretch()
    {
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        _childTransform.localRotation = Quaternion.identity;
    }

    private void ApplyStretch( Vector2 direction, float stretch )
    {
        var angle = Mathf.Atan2( direction.y, direction.x ) * Mathf.Rad2Deg;
        var scale = new Vector3( stretch, 1 / stretch, 1 );
        var rotation = Quaternion.AngleAxis( angle, Vector3.forward );

        transform.localScale = scale;
        transform.localRotation = rotation;
        _childTransform.localRotation = Quaternion.Inverse( rotation );

        if ( CompensateY )
        {
            var offset = (Mathf.Lerp( stretch, 1 / stretch, Mathf.Abs( Mathf.Sin( angle * Mathf.Deg2Rad ) ) ) * _childBounds.size.y -
                         _childBounds.size.y) * 0.5f; 
            var pos = transform.position;
            pos.y -= offset;
            _childTransform.position = pos;
        }
    }
}