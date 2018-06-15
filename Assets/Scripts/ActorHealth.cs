using Gamelogic.Extensions;
using UnityEngine;

public class ActorHealth : MonoBehaviour
{
    public int TotalHitCount = 3;

    [ ReadOnly ]
    public int CurrentHitCount = 3;

    public delegate void ActorHitHandler( ActorHealth actor, GameObject source );

    public event ActorHitHandler HitEvent;

    public bool IsAlive
    {
        get { return CurrentHitCount > 0; }
    }

    public void AccountDamages( int amount, GameObject source )
    {
        CurrentHitCount = Mathf.Max( 0, CurrentHitCount - amount );
        OnHitEvent( this, source );
        SlowMotionFx.Freeze();
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