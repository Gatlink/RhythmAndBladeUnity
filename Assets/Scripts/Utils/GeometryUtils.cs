﻿using System;
using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtils
{
    public static Vector2 ProjectPointToSegment( Vector2 p, Vector2 a, Vector2 b )
    {
        var sqrMagnitude = ( b - a ).sqrMagnitude;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if ( sqrMagnitude == 0.0 )
            return a;
        var t = Vector2.Dot( p - a, b - a ) / sqrMagnitude;
        if ( t < 0.0 )
            return a;
        if ( t > 1.0 )
            return b;

        return a + t * ( b - a );
    }

    public static float SqrDistanceToSegment( Vector2 p, Vector2 a, Vector2 b )
    {
        return ( ProjectPointToSegment( p, a, b ) - p ).sqrMagnitude;
    }

    public static float DistanceToSegment( Vector2 p, Vector2 a, Vector2 b )
    {
        return ( ProjectPointToSegment( p, a, b ) - p ).magnitude;
    }

    public static Vector2 ClosestPointToPolyLine( Vector2 p, List<Vector2> vertices )
    {
        if ( vertices.Count == 1 )
        {
            return vertices[ 0 ];
        }

        var minDistance = SqrDistanceToSegment( p, vertices[ 0 ], vertices[ 1 ] );
        var minDistanceSegmentIndex = 0;
        for ( var i = 2; i < vertices.Count; ++i )
        {
            var line = SqrDistanceToSegment( p, vertices[ i - 1 ], vertices[ i ] );
            if ( line < minDistance )
            {
                minDistance = line;
                minDistanceSegmentIndex = i - 1;
            }
        }

        var segmentStart = vertices[ minDistanceSegmentIndex ];
        var segmentEnd = vertices[ minDistanceSegmentIndex + 1 ];

        return ProjectPointToSegment( p, segmentStart, segmentEnd );
    }
    
    public static void DebugCollider( Collider2D hitbox )
    {
        var circleCollider2D = hitbox as CircleCollider2D;
        if ( circleCollider2D != null )
        {
            var matrix = circleCollider2D.transform.localToWorldMatrix;
            DebugExtension.DebugCircle( matrix.MultiplyPoint3x4(circleCollider2D.offset), Vector3.forward, circleCollider2D.radius );
            return;
        }

        var boxCollider2D = hitbox as BoxCollider2D;
        if ( boxCollider2D != null )
        {
            DebugExtension.DebugLocalCube( boxCollider2D.transform, boxCollider2D.size, boxCollider2D.offset );
            return;
        }

        throw new NotSupportedException( hitbox.GetType() + " not supported" );
    }
}