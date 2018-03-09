using Controllers;
using UnityEditor;
using UnityEngine;

[ CustomPropertyDrawer( typeof( BossControllerBase.Action ) ) ]
public class BossActionDrawer : PropertyDrawer
{
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        using ( new EditorGUI.PropertyScope( position, label, property ) )
        {
            const int typeWidth = 64;
            const int labelWidth = 64;

            var contentPosition = position;

            EditorGUI.indentLevel = 0;
            var typeProp = property.FindPropertyRelative( "Type" );
            var typeValue = (BossControllerBase.ActionType) typeProp.enumValueIndex;

            var needDuration = !( typeValue == BossControllerBase.ActionType.Charge ||
                                  typeValue == BossControllerBase.ActionType.Jump );
            if ( needDuration )
            {
                contentPosition.width = typeWidth;
            }

            typeValue = (BossControllerBase.ActionType) EditorGUI.EnumPopup( contentPosition, GUIContent.none,
                typeValue );
            typeProp.enumValueIndex = (int) typeValue;

            if ( !needDuration )
            {
                return;
            }

            contentPosition.x += typeWidth;
            contentPosition.width = ( position.width - typeWidth ) / 2;
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "Duration" ),
                new GUIContent( "Duration" ) );

            contentPosition.x += contentPosition.width;
            EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "DurationStdDev" ),
                new GUIContent( "Deviation" ) );
        }
    }
}