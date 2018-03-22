using UnityEditor;
using UnityEngine;

//[ CustomPropertyDrawer( typeof( WeightedActionsBossController.WeightedAction ) ) ]
public class WeightedActionDrawer : PropertyDrawer
{
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        using ( new EditorGUI.PropertyScope( position, label, property ) )
        {
            var contentPosition = position;

            EditorGUI.indentLevel = 0;

            const int probWidth = 48;
            const int spacing = 2;
            contentPosition.width = probWidth;
            EditorGUIUtility.labelWidth = 16;
            EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "Weight" ),
                new GUIContent( "P" ) );
            
            contentPosition.x += contentPosition.width + spacing;            
            contentPosition.width = position.width - probWidth - spacing;
            EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "Action" ), GUIContent.none );
        }
    }
}