using System.Linq;
using UnityEngine;

public class CollisionTrigger : MonoBehaviour
{
    public GameObject[] TargetGameObjects;
    public Behaviour[] TargetBehaviours;

    [ Tooltip( "Defaults to Player transform when null" ) ]
    public Transform TrackingTarget;

    [ Tooltip( "Also turn target state back to on (off) when TrackingTarget go out of Trigger" ) ]
    public bool AutoReset;

    [ Tooltip( "Set Targets to this state when TrackTarget enters Trigger" ) ]
    public bool StateWhenInCollider = true;

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

    private void Update()
    {
        if ( !_targetEnteredTrigger )
        {
            if ( IsTargetInTrigger() )
            {
                _targetEnteredTrigger = true;
                SetTargetsState( StateWhenInCollider );
            }
        }
        else if ( AutoReset && !IsTargetInTrigger() )
        {
            _targetEnteredTrigger = false;
            SetTargetsState( !StateWhenInCollider );
        }
    }

    private void SetTargetsState( bool on )
    {
        foreach ( var component in TargetBehaviours )
        {
            component.enabled = on;
        }

        foreach ( var obj in TargetGameObjects )
        {
            obj.SetActive( on );
        }
    }

    private bool IsTargetInTrigger()
    {
        return _triggers.Any( trigger => trigger.OverlapPoint( TrackingTarget.position ) );
    }
}