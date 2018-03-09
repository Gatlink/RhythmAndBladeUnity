using System.Linq;
using UnityEngine;

public class CollisionTrigger : EventTrigger
{
    [ Tooltip( "Defaults to Player transform when null" ) ]
    public Transform TrackingTarget;

    private Collider2D[] _triggers;
    private bool _targetEnteredTrigger;

    private void Start()
    {
        if ( TrackingTarget == null )
        {
            TrackingTarget = GameObject.FindGameObjectWithTag( Tags.Player ).transform;
        }

        _triggers = GetComponentsInChildren<Collider2D>();
    }

    protected override bool IsEventTriggered()
    {
        return _triggers.Any( trigger => trigger.OverlapPoint( TrackingTarget.position ) );
    }
}