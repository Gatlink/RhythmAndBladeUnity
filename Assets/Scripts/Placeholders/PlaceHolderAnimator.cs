using ActorStates;
using Gamelogic.Extensions;
using UnityEngine;

public class PlaceHolderAnimator : GLMonoBehaviour
{
    private Actor _actor;
    private Transform _muzzle;
    private Vector3 _initialScale;
    private CircleCollider2D _hitbox;
    private Vector2 _initialOffset;

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
        _actor = GetComponentInParent<Actor>();
        _actor.StateChangeEvent += StateChangeHandler;
        
        _muzzle = transform.Find( "View/Ball/Muzzle" );
        _initialScale = _muzzle.localScale;
        
        _initialOffset = Hitbox.offset;
    }

    private void Update()
    {
        _muzzle.localScale = _initialScale.WithX( _initialScale.x * _actor.Direction );
        Hitbox.offset = _initialOffset.WithX( _initialOffset.x * _actor.Direction );        
    }
    
    private void StateChangeHandler( IActorState previous, IActorState next )
    {
        if ( next is AttackState )
        {
            _hitbox.enabled = true;
            Invoke( () => _hitbox.enabled = false, ( next as AttackState ).HitDuration );
        }   
    }

    private void OnDrawGizmos()
    {
        if ( Hitbox != null && Hitbox.enabled )
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere( Hitbox.transform.position + (Vector3)Hitbox.offset, Hitbox.radius );
        }
    }
}