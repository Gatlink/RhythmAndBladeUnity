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
    public List<Vector2> Points;

    private void Reset()
    {
        if ( Points == null )
            Points = new List<Vector2>();

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

    public Bounds Bounds
    {
        get
        {
            var pos = (Vector2) transform.position;
            return Points.Aggregate<Vector2, Bounds>( new Bounds( Points[ 0 ] + pos, Vector2.zero ),
                ( b, p ) =>
                {
                    b.Encapsulate( p + pos );
                    return b;
                } );
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

    private void MoveTransform( Vector2 delta )
    {
        transform.position += (Vector3) delta;
        for ( var i = 0; i < Points.Count; i++ )
        {
            Points[ i ] -= delta;
        }
    }

    public IEnumerable<Segment> EnumerateSegments()
    {
        var points = Points;
        var position = (Vector2) transform.position;

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

    public Vector2 GetNearestPoint( Vector2 point )
    {
        var pos = (Vector2) transform.position;
        return GeometryUtils.ClosestPointToPolyLine( point - pos, Points ) + pos;
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