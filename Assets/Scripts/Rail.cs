using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Gamelogic.Extensions;
using UnityEngine.Serialization;

[ CheckInspectorButtons ]
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

            // ReSharper disable once RedundantTypeArgumentsOfMethod
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
        // ReSharper disable once RedundantCast
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

    public Vector2 EvaluatePosition( float normalizedPosition )
    {
        if ( normalizedPosition <= 0 )
        {
            return Points[ 0 ] + (Vector2) transform.position;
        }

        var sum = EnumerateSegments().Sum( segment => segment.Length );
        var linearPosition = sum * normalizedPosition;

        foreach ( var segment in EnumerateSegments() )
        {
            if ( linearPosition < segment.Length )
            {
                return Vector2.Lerp( segment.From, segment.To, linearPosition / segment.Length );
            }

            linearPosition -= segment.Length;
        }

        return Points[ Points.Count - 1 ] + (Vector2) transform.position;
    }

    public float EvaluateNormalizedPosition( Vector2 positionOnRail )
    {
        var pos = positionOnRail - (Vector2) transform.position;

        var points = Points;

        // find containing segment
        var minDistance = GeometryUtils.SqrDistanceToSegment( pos, points[ 0 ], points[ 1 ] );
        var minDistanceSegmentIndex = 0;
        for ( var i = 2; i < points.Count; ++i )
        {
            var line = GeometryUtils.SqrDistanceToSegment( pos, points[ i - 1 ], points[ i ] );
            if ( line < minDistance )
            {
                minDistance = line;
                minDistanceSegmentIndex = i - 1;
            }
        }

        var lengthBeforeSegment = 0f;
        for ( int i = 0; i < minDistanceSegmentIndex; i++ )
        {
            lengthBeforeSegment += Vector2.Distance( points[ i + 1 ], points[ i ] );
        }

        var totalLength = lengthBeforeSegment;
        for ( int i = minDistanceSegmentIndex; i < points.Count - 1; i++ )
        {
            totalLength += Vector2.Distance( points[ i + 1 ], points[ i ] );
        }

        return ( lengthBeforeSegment + Vector2.Distance( points[ minDistanceSegmentIndex ], pos ) ) / totalLength;
    }

    public Segment GetNearestSegment( Vector2 point )
    {
        var root = (Vector2) transform.position;
        var points = Points;
        point -= root;

        // find containing segment
        var minDistance = GeometryUtils.SqrDistanceToSegment( point, points[ 0 ], points[ 1 ] );
        var minDistanceSegmentIndex = 0;
        for ( var i = 2; i < points.Count; ++i )
        {
            var line = GeometryUtils.SqrDistanceToSegment( point, points[ i - 1 ], points[ i ] );
            if ( line < minDistance )
            {
                minDistance = line;
                minDistanceSegmentIndex = i - 1;
            }
        }

        return new Segment( points[ minDistanceSegmentIndex ] + root, points[ minDistanceSegmentIndex + 1 ] + root,
            minDistanceSegmentIndex );
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