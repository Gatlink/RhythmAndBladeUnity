using System;
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

            if ( typeValue != BossControllerBase.ActionType.Charge && typeValue != BossControllerBase.ActionType.JumpAttack )
            {
                contentPosition.width = typeWidth;
            }

            typeValue = (BossControllerBase.ActionType) EditorGUI.EnumPopup( contentPosition, GUIContent.none,
                typeValue );
            typeProp.enumValueIndex = (int) typeValue;

            if ( typeValue == BossControllerBase.ActionType.Charge ||
                 typeValue == BossControllerBase.ActionType.JumpAttack )
            {
                return;
            }

            contentPosition.x += typeWidth;
            contentPosition.width = position.width - typeWidth;

            EditorGUIUtility.labelWidth = labelWidth;

            switch ( typeValue )
            {
                case BossControllerBase.ActionType.Wait:
                    EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "DurationParameter" ),
                        new GUIContent( "Duration" ) );
                    break;
                case BossControllerBase.ActionType.Move:
                    EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "TargetTypeParameter" ),
                        new GUIContent( "Target" ) );
                    break;
                case BossControllerBase.ActionType.Attack:
                    EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "CountParameter" ),
                        new GUIContent( "Count" ) );
                    break;
                case BossControllerBase.ActionType.Count:
                case BossControllerBase.ActionType.JumpAttack:
                case BossControllerBase.ActionType.Charge:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}