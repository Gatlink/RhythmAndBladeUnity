using System.Collections.Generic;
using Gamelogic.Extensions;
using UnityEngine;

public class DestroyWhenHit : MonoBehaviour, IDestructible
{
    public Transform Root;
    public int HitCount = 1;

    [ SerializeField, ReadOnly ]
    private int _hitRemaining;

    private readonly HashSet<uint> _hits = new HashSet<uint>();

    public delegate void HitEventHandler();
    public event HitEventHandler HitEvent;

    private void Start()
    {
        _hitRemaining = HitCount;
    }

    public void Hit( HitInfo hitInfo )
    {
        if ( _hits.Contains( hitInfo.Id ) ) return;
        
        _hitRemaining = Mathf.Max( 0, _hitRemaining - hitInfo.Damage );
        _hits.Add( hitInfo.Id );
        
        SlowMotionFx.Freeze();

        if ( _hitRemaining <= 0 )
        {
            DestroyTarget();
        }
        else if ( HitEvent != null )
        {
            HitEvent();
        }
    }

    private void DestroyTarget()
    {
        var target = Root;
        if ( target == null )
        {
            target = transform;
        }

        Destroy( target.gameObject );
    }
}