using System.Linq;
using Gamelogic.Extensions;
using UnityEngine;

public class ActorHealth : MonoBehaviour
{
    public int TotalHitCount = 3;

    [ ReadOnly ]
    public int CurrentHitCount = 3;

    public delegate void ActorHitHandler( ActorHealth actor, GameObject source );

    public event ActorHitHandler HitEvent;
    
    private readonly Collider2D[] _colliderBuffer = new Collider2D[ 5 ];

    private ContactFilter2D _hurtContactFilter2D;

    public Collider2D CheckDamages()
    {
        foreach ( var hurtbox in GetComponentsInChildren<Collider2D>()
            .Where( col => col.CompareTag( Tags.Hurtbox ) && col.enabled ) )
        {
            var hitCount = hurtbox.OverlapCollider( _hurtContactFilter2D, _colliderBuffer );
            if ( hitCount > 0 )
            {
                return _colliderBuffer[ 0 ];
            }
        }

        return null;
    }

    public void AccountDamages( int amount, GameObject source )
    {
        CurrentHitCount = Mathf.Max( 0, CurrentHitCount - amount );
        OnHitEvent( this, source );
    }

    
    private void Awake()
    {
        _hurtContactFilter2D = new ContactFilter2D();
        _hurtContactFilter2D.NoFilter();
        _hurtContactFilter2D.SetLayerMask( 1 << LayerMask.NameToLayer( Layers.Harmfull ) );
    }

    private void Start()
    {
        CurrentHitCount = TotalHitCount;
    }

    private void OnHitEvent( ActorHealth actor, GameObject source )
    {
        var handler = HitEvent;
        if ( handler != null ) handler( actor, source );
    }
}