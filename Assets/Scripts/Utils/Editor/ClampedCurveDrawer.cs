using UnityEditor;
using UnityEngine;

[ CustomPropertyDrawer( typeof( ClampedCurveAttribute ) ) ]
public class ClampedCurveDrawer : PropertyDrawer
{
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField( position, property, label );
        if ( EditorGUI.EndChangeCheck() )
        {
            var attr = (ClampedCurveAttribute) attribute;
            var curve = property.animationCurveValue;
            ClampCurve( curve, attr.MinX, attr.MaxX, attr.MinY, attr.MaxY );
            property.animationCurveValue = curve;
        }
    }

    public static void ClampCurve( AnimationCurve curve, float minX, float maxX, float minY, float maxY )
    {
        var keys = curve.keys;

        for ( int i = 0; i < keys.Length; i++ )
        {
            keys[ i ].time = Mathf.Clamp( keys[ i ].time, minX, maxX );
            keys[ i ].value = Mathf.Clamp( keys[ i ].value, minY, maxY );
        }

		keys[ 0 ].time = minX;
		keys[ keys.Length - 1 ].time = maxX;

        curve.keys = keys;
    }
}