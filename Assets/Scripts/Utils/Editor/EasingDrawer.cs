using UnityEditor;
using UnityEngine;

[ CustomPropertyDrawer( typeof( Easing ) ) ]
public class EasingDrawer : PropertyDrawer
{
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        using ( var scope = new EditorGUI.PropertyScope( position, label, property ) )
        {
            var contentPosition = EditorGUI.PrefixLabel( position, scope.content );

            const float typeWidthRatio = 0.25f;

            contentPosition.width *= typeWidthRatio;
            EditorGUI.indentLevel = 0;
            var typeProp = property.FindPropertyRelative( "Type" );
            var easeType = (Easing.EasingType) EditorGUI.EnumPopup( contentPosition, GUIContent.none,
                (Easing.EasingType) typeProp.enumValueIndex );
            typeProp.enumValueIndex = (int) easeType;

            contentPosition.x += contentPosition.width;
            contentPosition.width *= 3;
            if ( easeType == Easing.EasingType.Curve )
            {
                EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "Curve" ), GUIContent.none );
            }
            else
            {
                var funcProp = property.FindPropertyRelative( "Function" );
                var funcType = (EasingFunction.Ease) EditorGUI.EnumPopup( contentPosition, GUIContent.none,
                    (EasingFunction.Ease) funcProp.enumValueIndex );
                funcProp.enumValueIndex = (int) funcType;
            }
        }
    }
}