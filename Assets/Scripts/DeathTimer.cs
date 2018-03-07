using System.Collections;
using System.Linq;
using UnityEngine;

public class DeathTimer : MonoBehaviour
{
    public delegate void TickHandler( bool dealsDamage );

    public event TickHandler TickEvent;
    
    public float TimeoutTicks = 10;
    public int DamagePerTick = 1;
    public float TickPeriodSeconds = 3;

    private Mobile _player;
    private Collider2D[] _zoneColliders;

    private IEnumerator Start()
    {
        _player = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Mobile>();
        _zoneColliders = GetComponentsInChildren<Collider2D>();

        yield return new WaitUntil( PlayerInZone );

        for ( int i = 0; i < TimeoutTicks; i++ )
        {
            if ( PlayerInZone() )
            {
                OnTickEvent( false );
            }
            yield return new WaitForSeconds( TickPeriodSeconds );
        }

        // deal damage every ticks when player is in zone
        while ( true )
        {
            if ( PlayerInZone() )
            {                
                _player.GetComponent<ActorHealth>().AccountDamages( DamagePerTick, gameObject );
                OnTickEvent( true );
            }
            yield return new WaitForSeconds( TickPeriodSeconds );
        }
    }

    private bool PlayerInZone()
    {
        return _zoneColliders.Any( col => col.OverlapPoint( _player.BodyPosition ) );
    }

    private void OnTickEvent( bool dealsdamage )
    {
        var handler = TickEvent;
        if ( handler != null ) handler( dealsdamage );
    }
}