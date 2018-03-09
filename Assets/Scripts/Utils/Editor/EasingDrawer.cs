using UnityEditor;
using UnityEngine;

[ CustomPropertyDrawer( typeof( Easing ) ) ]
public class EasingDrawer : PropertyDrawer
{
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        using ( var scope = new EditorGUI.PropertyScope( position, label, property ) )
        {
            EditorGUILayout.PrefixLabel( scope.content );

            using ( new EditorGUI.IndentLevelScope( 1 ) )
            {
                var typeProp = property.FindPropertyRelative( "Type" );
                var easeType = (Easing.EasingType) EditorGUILayout.EnumPopup( "Type",
                    (Easing.EasingType) typeProp.enumValueIndex );
                typeProp.enumValueIndex = (int) easeType;

                if ( easeType == Easing.EasingType.Curve )
                {
                    EditorGUILayout.PropertyField( property.FindPropertyRelative( "Curve" ) );
                }
                else
                {
                    var funcProp = property.FindPropertyRelative( "Function" );
                    var funcType = (EasingFunction.Ease) EditorGUILayout.EnumPopup( "Function",
                        (EasingFunction.Ease) funcProp.enumValueIndex );
                    funcProp.enumValueIndex = (int) funcType;
                }
            }
        }
    }
}