using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Gamelogic.Extensions;
using UnityEngine.Serialization;

public class Rail : GLMonoBehaviour
{
    public const float SlopeLimit = 35;

    private static readonly List<Rail> All = new List<Rail>();

    public bool Closed;

    [ HideInInspector, SerializeField ]
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
        All.Add( this );
    }

    private void Start()
    {
        CreateColliders();
    }

    public Bounds Bounds
    {
        get
        {
            return Points.Aggregate<Vector3, Bounds>( new Bounds( Points[ 0 ], Vector3.zero ), ( b, v ) =>
            {
                b.Encapsulate( v );
                return b;
            } );
        }
    }

    private EdgeCollider2D NewCollider( Segment first )
    {
        var edgeCollider = new GameObject( first.IsWall() ? "Wall" : "Rail Part", typeof( EdgeCollider2D ) )
            .GetComponent<EdgeCollider2D>();
        edgeCollider.transform.SetParent( transform, true );
        edgeCollider.gameObject.layer = LayerMask.NameToLayer( first.IsWall() ? Layers.Wall : Layers.Ground );
        edgeCollider.points = new Vector2[] { first.From, first.To };
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
                currentCollider.points = currentCollider.points.Concat( Enumerable.Repeat( segment.To, 1 ) )
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

    public IEnumerable<Segment> EnumerateSegments()
    {
        var points = Points;
        var position = transform.position;

        for ( int i = 1; i < points.Count; i++ )
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