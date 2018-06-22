using UnityEngine;

public static class SimpleDrawing
{
    private static Material _drawMaterial;

    private static void CreateDrawMaterial()
    {
        if ( _drawMaterial == null )
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            var shader = Shader.Find( "Hidden/Internal-Colored" );
            _drawMaterial = new Material( shader );
            _drawMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            _drawMaterial.SetInt( "_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha );
            _drawMaterial.SetInt( "_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha );
            // Turn backface culling off
            _drawMaterial.SetInt( "_Cull", (int) UnityEngine.Rendering.CullMode.Off );
            // Turn off depth writes
            _drawMaterial.SetInt( "_ZWrite", 0 );
        }
    }

    public static void DrawLine( Vector3 from, Vector3 to )
    {
        DrawLine( from, to, Color.white );
    }

    public static void DrawLine( Vector3 from, Vector3 to, Color color )
    {
        CreateDrawMaterial();

        // Apply the material
        _drawMaterial.SetPass( 0 );

        GL.Begin( GL.LINES );

        GL.Color( color );
        GL.Vertex3( from.x, from.y, from.z );
        GL.Vertex3( to.x, to.y, to.z );

        GL.End();
    }

    public static void DrawDisk( Vector3 center, float radius )
    {
        DrawDisk( center, radius, Vector3.back );
    }

    public static void DrawDisk( Vector3 center, float radius, Vector3 normal )
    {
        DrawDisk( center, radius, normal, Color.yellow );
    }

    public static void DrawDisk( Vector3 center, float radius, Color color )
    {
        DrawDisk( center, radius, Vector3.back, color );
    }

    public static void DrawDisk( Vector3 center, float radius, Vector3 normal, Color color )
    {
        CreateDrawMaterial();

        // Apply the material
        _drawMaterial.SetPass( 0 );

        GL.Begin( GL.TRIANGLES );

        GL.Color( color );

        var localRight = radius * normal.RandomNormal().normalized;
        var localUp = radius * Vector3.Cross( normal, localRight ).normalized;

        const int n = 12;
        const float angle = 2 * Mathf.PI / n;

        var vertices = new Vector3[ n ];
        for ( var i = 0; i < n; i++ )
        {
            vertices[ i ] = center + Mathf.Cos( i * angle ) * localRight + Mathf.Sin( i * angle ) * localUp;
        }

        Vector3 v1, v2;
        
        for ( var i = 0; i < n - 1; i++ )
        {
            v1 = vertices[ i ];
            v2 = vertices[ i + 1];
            GL.Vertex3( center.x, center.y, center.z );
            GL.Vertex3( v1.x, v1.y, v1.z );
            GL.Vertex3( v2.x, v2.y, v2.z );
        }

        v1 = vertices[ n - 1 ];
        v2 = vertices[ 0 ];
        GL.Vertex3( center.x, center.y, center.z );
        GL.Vertex3( v1.x, v1.y, v1.z );
        GL.Vertex3( v2.x, v2.y, v2.z );

        GL.End();
    }

    private static Vector3 RandomNormal( this Vector3 self )
    {
        const float eps = 1e-4f;
        var v = Vector3.up;
        var normal = Vector3.Cross( v, self );
        if ( normal.sqrMagnitude <= eps )
        {
            v = Vector3.forward;
            normal = Vector3.Cross( v, self );
        }
        return normal;
    }
}