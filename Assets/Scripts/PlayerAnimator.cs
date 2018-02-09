using ActorStates;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public float HorizontalSpeedThreshold = 0.1f;

    private Actor _actor;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _actor = GetComponentInParent<Actor>();
        _animator = GetComponent<Animator>();
        _actor.StateChangeEvent += StateChangeHandler;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        var horizontalSpeed = Mathf.Max( 0, Mathf.Abs( _actor.CurrentVelocity.x ) - HorizontalSpeedThreshold );
        _animator.SetFloat( "HorizontalSpeed", horizontalSpeed );
        _animator.SetFloat( "HorizontalAcceleration", _actor.CurrentAcceleration.x * Mathf.Sign( _actor.CurrentVelocity.x ) );
        
        _spriteRenderer.flipX = _actor.Direction < 0;
        
        Vector2 normal;
        var grounded = _actor.CheckGround( out normal, snap: false );
        _animator.SetBool( "Grounded", grounded);

        if ( grounded )
        {
            transform.localRotation = Quaternion.FromToRotation( Vector3.up, normal );
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }
    }

    private void StateChangeHandler( IActorState previous, IActorState next )
    {
        _animator.SetBool( "Jumping", next is JumpState );
        _animator.SetBool( "Dashing", next is DashState );
        _animator.SetBool( "Sliding", next is WallSlideState );
    }
}