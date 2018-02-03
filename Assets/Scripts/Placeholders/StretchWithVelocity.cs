using Gamelogic.Extensions;
using UnityEngine;

public class StretchWithVelocity : MonoBehaviour
{
    public float MaxVelocity = 10;
    public float MaxSpeedStretch = 0.5f;
    public bool CompensatePosition = true;
    public float MinVelocity = 0.1f;

    public float Smoothing = 0.1f;

    private Vector2 _lastPosition;
    private Transform _childTransform;
    private Bounds _childBounds;

    private float _currentStretchVelocity;
    private float _targetStretch;
    private float _currentStretch;

    private Vector2 _currentDirection;

    [ SerializeField, ReadOnly ]
    private Vector2 _percievedVelocity;

    private void Awake()
    {
        _childTransform = transform.GetChild( 0 );
    }

    private void Start()
    {
        _childBounds = _childTransform.GetComponent<Renderer>().bounds;
        _currentDirection = Vector2.right;
    }

    private void OnEnable()
    {
        _lastPosition = transform.position;
        _currentStretch = 1;
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

        if ( velocity.sqrMagnitude >= MinVelocity * MinVelocity )
        {
            _currentDirection = velocity;
        }
        else
        {
            _currentDirection = Vector2.right;
        }

        _percievedVelocity = velocity;
        _lastPosition = position;

        _targetStretch = 1 + Mathf.Lerp( 0, MaxSpeedStretch, Mathf.Clamp01( velocity.magnitude / MaxVelocity ) );
        _currentStretch = Mathf.SmoothDamp( _currentStretch, _targetStretch, ref _currentStretchVelocity, Smoothing );

        ApplyStretch( _currentDirection, _currentStretch );
    }

    private void ResetStretch()
    {
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        _childTransform.localRotation = Quaternion.identity;
        _currentStretch = 1;
    }

    private void ApplyStretch( Vector2 direction, float stretch )
    {
        var angle = Mathf.Atan2( direction.y, direction.x ) * Mathf.Rad2Deg;
        var scale = new Vector3( stretch, 1 / stretch, 1 );
        var rotation = Quaternion.AngleAxis( angle, Vector3.forward );

        transform.localScale = scale;
        transform.localRotation = rotation;
        _childTransform.localRotation = Quaternion.Inverse( rotation );

        if ( CompensatePosition )
        {
            var offsetX = Mathf.Abs( Mathf.Cos( angle * Mathf.Deg2Rad ) ) * ( 1 - 1 / stretch ) * _childBounds.size.y *
                          0.5f;
            var offsetY = Mathf.Sin( angle * Mathf.Deg2Rad ) * ( stretch - 1 ) * _childBounds.size.y * 0.5f;
            var pos = transform.position;
            pos.y -= offsetX;
            pos.y -= offsetY;
            _childTransform.position = pos;
        }
    }
}