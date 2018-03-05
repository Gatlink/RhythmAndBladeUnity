using ActorStates;
using UnityEngine;

public abstract class ActorAnimator<TActor> : MonoBehaviour where TActor : ActorBase<TActor>
{
    public float HorizontalSpeedThreshold = 0.1f;

    protected ActorBase<TActor> Actor;
    private Mobile _mobile;
    protected Animator Animator;

    private void Start()
    {
        Actor = GetComponentInParent<TActor>();
        Actor.StateChangeEvent += StateChangeHandler;
        _mobile = GetComponentInParent<Mobile>();
        Animator = GetComponent<Animator>();
    }

    protected virtual void LateUpdate()
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