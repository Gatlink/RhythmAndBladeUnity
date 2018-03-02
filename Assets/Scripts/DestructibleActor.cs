using System.Collections.Generic;
using UnityEngine;

public class DestructibleActor : MonoBehaviour, IDestructible
{
    private ActorHealth _health;

    private readonly HashSet<uint> _hits = new HashSet<uint>();

    private void Start()
    {
        _health = GetComponentInParent<ActorHealth>();
    }

    public void Hit( HitInfo hitInfo )
    {
        if ( _hits.Contains( hitInfo.Id ) ) return;
        
        _hits.Add( hitInfo.Id );
        
        _health.AccountDamages( hitInfo.Damage );
    }
}