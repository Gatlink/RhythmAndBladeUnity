using UnityEngine;
using UnityEngine.UI;

public class HitPointsHUD : MonoBehaviour
{
    public ActorHealth Target;
    public string TargetTag;
    private GameObject _hitPointPrefab;

    private void Start()
    {
        _hitPointPrefab = transform.GetChild( 0 ).gameObject;
        _hitPointPrefab.transform.SetParent( null );

        if ( Target == null )
        {
            var targetObject = GameObject.FindGameObjectWithTag( TargetTag );
            if ( targetObject != null )
            {
                Target = targetObject.GetComponent<ActorHealth>();                              
            }
        }

        if ( Target == null )
        {
            enabled = false;
            return;
        }

        InitializeHitPoints();
        Target.HitEvent += OnTargetHit;
    }

    private void OnTargetHit( ActorHealth actor )
    {
        var container = transform;
        var total = actor.TotalHitCount;
        var current = actor.CurrentHitCount;

        for ( var i = 0; i < total; i++ )
        {
            container.GetChild( i ).Find( "Fill" ).GetComponent<Image>().enabled = i < current;
        }
    }

    private void InitializeHitPoints()
    {
        var container = transform;
        var total = Target.TotalHitCount;
        for ( var i = 0; i < total; i++ )
        {
            var hitPoint = Instantiate( _hitPointPrefab );
            hitPoint.transform.SetParent( container, true );
        }
    }
}