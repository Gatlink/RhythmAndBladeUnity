using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class Rail : MonoBehaviour
{
// STATIC

    private static readonly List<Rail> All = new List<Rail>();

    public static bool GetRailProjection( Vector3 pos, out Vector3 result )
    {
        result = Vector3.zero;
        var points = new List<Vector3>();
        foreach ( var rail in All )
        {
            Vector3 onRail;
            if ( rail.GetClosestPointOnRail( pos, out onRail ) )
                points.Add( onRail );
        }

        if ( points.Count == 0 )
            return false;

        var lastDis = float.MaxValue;
        foreach ( var p in points )
        {
            var curDis = Vector2.SqrMagnitude( p - pos );
            if ( curDis < lastDis )
            {
                result = p;
                lastDis = curDis;
            }
        }

        return true;
    }

    public static List<float> GetAllCollidingWalls( Vector3 pos, float radius )
    {
        var walls = new List<float>();
        foreach ( var rail in All )
        {
            var collidingWalls = rail.GetCollidingWalls( pos, radius );
            walls.AddRange( collidingWalls );
        }

        return walls;
    }


// METHODS

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

    private void Awake()
    {
        All.Add( this );
    }

    private void Start()
    {
        CreateCollider();
    }

    private void CreateCollider()
    {
        var edgeCollider = new GameObject("Collider", typeof(EdgeCollider2D)).GetComponent<EdgeCollider2D>();
        edgeCollider.transform.SetParent( transform, false );
        edgeCollider.points = Points.Select( v => (Vector2) v ).ToArray();
        edgeCollider.gameObject.layer = gameObject.layer;
    }

    public bool GetProjection( int idx, Vector3 pos, out Vector3 proj )
    {
        proj = Vector3.negativeInfinity;
        if ( idx > Points.Count - 1 )
            return false;

        var prev = Points[ idx ] + transform.position;
        var next = Points[ idx + 1 ] + transform.position;

        if ( ( prev.x <= pos.x && next.x >= pos.x )
             || ( prev.x >= pos.x && next.x <= pos.x ) )
        {
            var a = ( next.y - prev.y ) / ( next.x - prev.x );
            var b = next.y - a * next.x;
            proj = new Vector3( pos.x, a * pos.x + b, 0f );
            return true;
        }

        return false;
    }

    public void GetPreviousAndNextPoint( Vector3 pos, out int prev, out int next )
    {
        prev = next = -1;
        if ( Points.Count == 0 )
            return;

        if ( Points.Count == 1 )
        {
            prev = next = 0;
            return;
        }

        var pts = new Dictionary<int, float>();
        for ( var i = 0; i < Points.Count - 1; ++i )
        {
            Vector3 proj;
            if ( GetProjection( i, pos, out proj ) )
                pts.Add( i, Vector2.Distance( proj, pos ) );
        }

        if ( pts.Count <= 0 )
            return;

        prev = pts.OrderBy( kvp => kvp.Value ).FirstOrDefault().Key;
        if ( prev >= 0 && prev < Points.Count - 1 )
            next = prev + 1;
    }

    public bool GetClosestPointOnRail( Vector3 pos, out Vector3 projection )
    {
        projection = Vector3.negativeInfinity;
        if ( Points.Count == 0 )
            return false;

        if ( Points.Count == 1 )
        {
            projection = Points[ 0 ];
            return true;
        }

        var pts = new List<Vector3>();
        for ( var i = 0; i < Points.Count - 1; ++i )
        {
            Vector3 proj;
            if ( GetProjection( i, pos, out proj ) )
                pts.Add( proj );
        }

        var lastSqrDistance = float.MaxValue;
        var result = false;
        foreach ( var point in pts )
        {
            var currentSqrDistance = Vector2.Distance( point, pos );
            if ( currentSqrDistance < lastSqrDistance )
            {
                projection = point;
                lastSqrDistance = currentSqrDistance;
                result = true;
            }
        }

        return result;
    }

    private List<float> GetCollidingWalls( Vector3 pos, float radius )
    {
        var result = new List<float>();
        if ( Points.Count < 2 )
            return result;

        for ( var i = 0; i < Points.Count - 1; ++i )
        {
            var prev = Points[ i ] + transform.position;
            var next = Points[ i + 1 ] + transform.position;
            if ( prev.x != next.x )
                continue;

            var wall = prev.x;
            if ( wall < pos.x - radius || wall > pos.x + radius )
                continue;

            var maxY = Mathf.Max( prev.y, next.y );
            var minY = Mathf.Min( prev.y, next.y );
            if ( maxY > pos.y && minY < pos.y )
                result.Add( wall );
        }

        return result;
    }

    public IEnumerable<Segment> EnumerateSegments()
    {
        var points = Points;
        var position = transform.position;

        for ( int i = 1; i < points.Count; i++ )
        {
            var from = points[ i - 1 ] + position;
            var to = points[ i ] + position;
            yield return new Segment() { From = from, To = to };
        }
    }

    public struct Segment
    {
        public Vector3 From;
        public Vector3 To;
    }
}