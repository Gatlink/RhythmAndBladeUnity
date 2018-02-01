using UnityEngine;

public class StretchWithVelocity : MonoBehaviour
{
    public float MaxVelocity = 10;
    public float MaxSpeedStretch = 0.5f;

    private Vector2 _lastPosition;
    private Transform _childTransform;

    private void Awake()
    {
        _childTransform = transform.GetChild( 0 );
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

        ApplyStretch( velocity, 1 + Mathf.Lerp( 0, MaxSpeedStretch, velocity.magnitude / MaxVelocity ) );
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
    }
}