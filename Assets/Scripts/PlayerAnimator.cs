using ActorStates;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public float HorizontalSpeedThreshold = 0.1f;

    private PlayerActor _actor;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _actor = GetComponentInParent<PlayerActor>();
        _actor.StateChangeEvent += StateChangeHandler;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        var mob = _actor.Mobile;
        var horizontalSpeed = Mathf.Max( 0, Mathf.Abs( mob.CurrentVelocity.x ) - HorizontalSpeedThreshold );
        _animator.SetFloat( "HorizontalSpeed", horizontalSpeed );
        _animator.SetFloat( "HorizontalAcceleration",
            mob.CurrentAcceleration.x * Mathf.Sign( mob.CurrentVelocity.x ) );

        _spriteRenderer.flipX = mob.Direction < 0;

        Vector2 normal;
        var grounded = mob.CheckGround( out normal, snap: false );

        if ( grounded )
        {
            transform.localRotation = Quaternion.FromToRotation( Vector3.up, normal );
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }
    }

    private void StateChangeHandler( IActorState<PlayerActor> previous, IActorState<PlayerActor> next )
    {
        if ( next is JumpState )
        {
            _animator.SetTrigger( "Jump" );
        }
        else if ( next is DashState )
        {
            _animator.SetTrigger( "Dash" );
        }
        else if ( next is WallSlideState )
        {
            _animator.SetTrigger( "Slide" );
        }
        else if ( next is FallState )
        {
            _animator.SetTrigger( "Fall" );
        }
        else if ( next is GroundedState )
        {
            _animator.SetTrigger( "Ground" );
        }
        else if ( next is AttackState )
        {
            var attackState = (AttackState) next;
            _animator.SetInteger( "Combo", attackState.ComboCount );
            _animator.SetTrigger( "Attack" );
        }
    }
}