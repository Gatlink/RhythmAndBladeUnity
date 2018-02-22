using UnityEngine;

public static class GeometryUtils
{
//    public static float DistanceToLine(Vector3 p1, Vector3 p2)
//    {
//        p1 = (Vector3) HandleUtility.WorldToGUIPoint(p1);
//        p2 = (Vector3) HandleUtility.WorldToGUIPoint(p2);
//        float num = HandleUtility.DistancePointLine((Vector3) Event.current.mousePosition, p1, p2);
//        if ((double) num < 0.0)
//            num = 0.0f;
//        return num;
//    }
//
//    public static Vector3 ClosestPointToPolyLine(params Vector3[] vertices)
//    {
//        if ( vertices.Length == 1 )
//        {
//            return vertices[ 0 ];
//        }
//
//        float num1 = HandleUtility.DistanceToLine(vertices[0], vertices[1]);
//        int index1 = 0;
//        for (int index2 = 2; index2 < vertices.Length; ++index2)
//        {
//            float line = HandleUtility.DistanceToLine(vertices[index2 - 1], vertices[index2]);
//            if ((double) line < (double) num1)
//            {
//                num1 = line;
//                index1 = index2 - 1;
//            }
//        }
//        Vector3 vertex1 = vertices[index1];
//        Vector3 vertex2 = vertices[index1 + 1];
//        Vector2 vector2_1 = Event.current.mousePosition - HandleUtility.WorldToGUIPoint(vertex1);
//        Vector2 vector2_2 = HandleUtility.WorldToGUIPoint(vertex2) - HandleUtility.WorldToGUIPoint(vertex1);
//        float magnitude = vector2_2.magnitude;
//        float num2 = Vector3.Dot((Vector3) vector2_2, (Vector3) vector2_1);
//        if ((double) magnitude > 9.99999997475243E-07)
//            num2 /= magnitude * magnitude;
//        float t = Mathf.Clamp01(num2);
//        return Vector3.Lerp(vertex1, vertex2, t);
//    }
}