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
        _animator.SetBool( "Grounded", _actor.CheckGround( snap: false ) );
        _spriteRenderer.flipX = _actor.Direction < 0;
    }

    private void StateChangeHandler( IActorState previous, IActorState next )
    {
        if ( next is JumpState )
        {
            _animator.SetTrigger( "Jump" );
        }
    }
}