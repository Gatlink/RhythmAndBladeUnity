using System.Linq;
using Gamelogic.Extensions;
using UnityEngine;

[ RequireComponent( typeof( Rail ) ) ]
public class RailCollider : GLMonoBehaviour
{
    [ Layer ]
    public int OverrideLayer;

    private Rail _rail;

    private void Awake()
    {
        _rail = GetRequiredComponent<Rail>();
        CreateColliders();
    }

    private EdgeCollider2D NewCollider( Rail.Segment first )
    {
        var edgeCollider = new GameObject( first.IsWall() ? "Wall" : "Rail Part", typeof( EdgeCollider2D ) )
            .GetComponent<EdgeCollider2D>();
        edgeCollider.transform.SetParent( transform, false );
        if ( OverrideLayer != 0 )
        {
            edgeCollider.gameObject.layer = OverrideLayer;
        }
        else
        {
            edgeCollider.gameObject.layer = LayerMask.NameToLayer( first.IsWall() ? Layers.Wall : Layers.Ground );
        }

        if ( this.GetInterfaceComponent<IMoving>() != null )
        {
            edgeCollider.gameObject.tag = Tags.Moving;
        }

        // ReSharper disable once RedundantExplicitArrayCreation
        edgeCollider.points = new Vector2[]
            { first.From - (Vector2) transform.position, first.To - (Vector2) transform.position };
        return edgeCollider;
    }

    private void CreateColliders()
    {
        var first = _rail.EnumerateSegments().First();
        var currentCollider = NewCollider( first );
        var currentColliderIsWall = first.IsWall();

        foreach ( var segment in _rail.EnumerateSegments().Skip( 1 ) )
        {
            if ( segment.IsWall() == currentColliderIsWall )
            {
                // add point to current collider
                currentCollider.points = currentCollider.points
                    .Concat( Enumerable.Repeat( segment.To - (Vector2) transform.position, 1 ) )
                    .ToArray();
            }
            else
            {
                currentCollider = NewCollider( segment );
                currentColliderIsWall = segment.IsWall();
            }
        }
    }
}