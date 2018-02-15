using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Gamelogic.Extensions;
using UnityEngine.Serialization;

public class Rail : GLMonoBehaviour
{
    public const float SlopeLimit = 35;

    public bool Closed;

    [ FormerlySerializedAs( "points" ) ]
    public List<Vector3> Points;

    private void Reset()
    {
        if ( Points == null )
            Points = new List<Vector3>();

        Points.Clear();
        Points.Add( new Vector2( -1, 0 ) );
        Points.Add( new Vector2( 1, 0 ) );
    }

    private void OnValidate()
    {
        if ( Points.Count < 2 )
        {
            Reset();
        }
    }

    private void Awake()
    {
        CreateColliders();
    }

    public Bounds Bounds
    {
        get
        {
            return Points.Aggregate<Vector3, Bounds>( new Bounds( Points[ 0 ] + transform.position, Vector3.zero ),
                ( b, p ) =>
                {
                    b.Encapsulate( p + transform.position );
                    return b;
                } );
        }
    }

    private EdgeCollider2D NewCollider( Segment first )
    {
        var edgeCollider = new GameObject( first.IsWall() ? "Wall" : "Rail Part", typeof( EdgeCollider2D ) )
            .GetComponent<EdgeCollider2D>();
        edgeCollider.transform.SetParent( transform, false );
        edgeCollider.gameObject.layer = LayerMask.NameToLayer( first.IsWall() ? Layers.Wall : Layers.Ground );
        if ( this.GetInterfaceComponent<IMoving>() != null )
        {
            edgeCollider.gameObject.tag = Tags.Moving;
        }

        edgeCollider.points = new Vector2[]
            { first.From - (Vector2) transform.position, first.To - (Vector2) transform.position };
        return edgeCollider;
    }

    private void CreateColliders()
    {
        var first = EnumerateSegments().First();
        var currentCollider = NewCollider( first );
        var currentColliderIsWall = first.IsWall();

        foreach ( var segment in EnumerateSegments().Skip( 1 ) )
        {
            if ( segment.IsWall() == currentColliderIsWall )
            {
                // add point to current collider
                currentCollider.points = currentCollider.points.Concat( Enumerable.Repeat( segment.To - (Vector2)transform.position, 1 ) )
                    .ToArray();
            }
            else
            {
                currentCollider = NewCollider( segment );
                currentColliderIsWall = segment.IsWall();
            }
        }
    }

    [ InspectorButton ]
    public void ReversePointsOrder()
    {
        Points.Reverse();
    }

    [ InspectorButton ]
    public void MoveTransformToFirstPoint()
    {
        var delta = Points[ 0 ];
        MoveTransform( delta );
    }

    [ InspectorButton ]
    public void MoveTransformToBoundsCenter()
    {
        var delta = Bounds.center - transform.position;
        MoveTransform( delta );
    }

    private void MoveTransform( Vector3 delta )
    {
        transform.position += delta;
        for ( var i = 0; i < Points.Count; i++ )
        {
            Points[ i ] -= delta;
        }
    }

    public IEnumerable<Segment> EnumerateSegments()
    {
        var points = Points;
        var position = transform.position;

        for ( var i = 1; i < points.Count; i++ )
        {
            var from = points[ i - 1 ] + position;
            var to = points[ i ] + position;
            yield return new Segment( from, to, i - 1 );
        }

        if ( Closed && points.Count >= 3 )
        {
            yield return new Segment( points[ points.Count - 1 ] + position, points[ 0 ] + position, points.Count - 1 );
        }
    }

    public struct Segment
    {
        public Vector2 From;
        public Vector2 To;

        public readonly int FromIndex;

        public Segment( Vector2 @from, Vector2 to, int fromIndex = -1 )
        {
            From = @from;
            To = to;
            FromIndex = fromIndex;
        }

        public bool IsWall()
        {
            return Mathf.Abs( From.x - To.x ) <= Mathf.Epsilon;
        }

        public Vector2 Center
        {
            get { return 0.5f * ( From + To ); }
        }

        public Vector2 Normal
        {
            get { return ( To - From ).Rotate90().normalized; }
        }

        public float Length
        {
            get { return Vector2.Distance( From, To ); }
        }

        public float Slope
        {
            get { return Mathf.Abs( 90 - Vector2.Angle( Vector2.up, To - From ) ); }
        }
    }
}