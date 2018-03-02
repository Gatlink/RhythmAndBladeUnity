using ActorStates;
using UnityEngine;

public abstract class ActorAnimator<TActor> : MonoBehaviour where TActor : ActorBase<TActor>
{
    public float HorizontalSpeedThreshold = 0.1f;

    private ActorBase<TActor> _actor;
    private Mobile _mobile;
    protected Animator Animator;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _actor = GetComponentInParent<TActor>();
        _actor.StateChangeEvent += StateChangeHandler;
        _mobile = GetComponentInParent<Mobile>();
        Animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        var horizontalSpeed = Mathf.Max( 0, Mathf.Abs( _mobile.CurrentVelocity.x ) - HorizontalSpeedThreshold );
        Animator.SetFloat( "HorizontalSpeed", horizontalSpeed );
        Animator.SetFloat( "HorizontalAcceleration",
            _mobile.CurrentAcceleration.x * Mathf.Sign( _mobile.CurrentVelocity.x ) );

        transform.localScale = new Vector3( _mobile.Direction, 1, 1 );

        Vector2 normal;
        var grounded = _mobile.CheckGround( out normal, snap: false );

        if ( grounded )
        {
            transform.localRotation = Quaternion.FromToRotation( Vector3.up, normal );
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }
    }

    protected abstract void StateChangeHandler( IActorState previous, IActorState next );
}