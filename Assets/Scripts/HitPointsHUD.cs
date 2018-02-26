using System;
using UnityEngine;
using UnityEngine.UI;

public class HitPointsHUD : MonoBehaviour
{
    public Actor Target;
    private GameObject _hitPointPrefab;

    private void Start()
    {
        if ( Target == null )
        {
            Target = GameObject.FindGameObjectWithTag( Tags.Player )
                .GetComponent<Actor>();
        }

        _hitPointPrefab = transform.GetChild( 0 ).gameObject;
        _hitPointPrefab.transform.SetParent( null );

        InitializeHitPoints();

        Target.HitEvent += OnTargetHit;
    }

    private void OnTargetHit( Actor actor )
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