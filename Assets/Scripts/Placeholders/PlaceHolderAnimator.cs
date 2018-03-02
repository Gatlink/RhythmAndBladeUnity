using ActorStates;
using ActorStates.Player;
using Gamelogic.Extensions;
using UnityEngine;

public class PlaceHolderAnimator : GLMonoBehaviour
{
    private PlayerActor _actor;
    private Transform _muzzle;
    private Vector3 _initialScale;
    private CircleCollider2D _hitbox;
    private Vector2 _initialOffset;
    private int _currentCombo;

    public CircleCollider2D Hitbox
    {
        get
        {
            if ( _hitbox == null )
            {
                _hitbox = transform.Find( "Hitbox" ).GetComponent<CircleCollider2D>();
            }

            return _hitbox;
        }
    }

    private void Start()
    {
        _actor = GetComponentInParent<PlayerActor>();
        _actor.StateChangeEvent += StateChangeHandler;

        _muzzle = transform.Find( "View/Ball/Muzzle" );
        _initialScale = _muzzle.localScale;

        _initialOffset = Hitbox.offset;
    }

    private void Update()
    {
        _muzzle.localScale = _initialScale.WithX( _initialScale.x * _actor.Mobile.Direction );
        Hitbox.offset = _initialOffset.WithX( _initialOffset.x * _actor.Mobile.Direction );
    }

    private void StateChangeHandler( IActorState previous, IActorState next )
    {
        var attackState = next as AttackState;
        if ( attackState != null )
        {
            _currentCombo = attackState.ComboCount;
            _hitbox.enabled = true;
            Invoke( () => _hitbox.enabled = false, attackState.HitDuration );
        }
    }

    private void OnDrawGizmos()
    {
        if ( Hitbox != null && Hitbox.enabled )
        {
            Gizmos.color = HitBoxColor( _currentCombo );
            Gizmos.DrawSphere( Hitbox.transform.position + (Vector3) Hitbox.offset, Hitbox.radius );
        }
    }

    private Color HitBoxColor( int combo )
    {
        switch ( combo )
        {
            case 2:
                return Color.yellow;
            case 1:
                return Color.blue;
            default:
                return Color.red;
        }
    }
}