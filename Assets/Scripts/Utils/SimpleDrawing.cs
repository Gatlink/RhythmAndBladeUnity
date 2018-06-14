using UnityEngine;

public static class SimpleDrawing
{
    private static Material _lineMaterial;

    private static void CreateLineMaterial()
    {
        if ( _lineMaterial == null )
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            var shader = Shader.Find( "Hidden/Internal-Colored" );
            _lineMaterial = new Material( shader );
            _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            _lineMaterial.SetInt( "_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha );
            _lineMaterial.SetInt( "_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha );
            // Turn backface culling off
            _lineMaterial.SetInt( "_Cull", (int) UnityEngine.Rendering.CullMode.Off );
            // Turn off depth writes
            _lineMaterial.SetInt( "_ZWrite", 0 );
        }
    }

    public static void DrawLine( Vector3 from, Vector3 to )
    {
        DrawLine( from, to, Color.white );
    }

    public static void DrawLine( Vector3 from, Vector3 to, Color color )
    {
        CreateLineMaterial();

        // Apply the line material
        _lineMaterial.SetPass( 0 );

        GL.Begin( GL.LINES );

        GL.Color( color );
        GL.Vertex3( from.x, from.y, from.z );
        GL.Vertex3( to.x, to.y, to.z );

        GL.End();
    }
}