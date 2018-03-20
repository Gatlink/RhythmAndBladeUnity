using System;
using Controllers;
using UnityEditor;
using UnityEngine;

[ CustomPropertyDrawer( typeof( BossActionControllerBase.Action ) ) ]
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
            var typeValue = (BossActionControllerBase.ActionType) typeProp.enumValueIndex;

            if ( typeValue != BossActionControllerBase.ActionType.Charge && typeValue != BossActionControllerBase.ActionType.JumpAttack )
            {
                contentPosition.width = typeWidth;
            }

            typeValue = (BossActionControllerBase.ActionType) EditorGUI.EnumPopup( contentPosition, GUIContent.none,
                typeValue );
            typeProp.enumValueIndex = (int) typeValue;

            if ( typeValue == BossActionControllerBase.ActionType.Charge ||
                 typeValue == BossActionControllerBase.ActionType.JumpAttack )
            {
                return;
            }

            contentPosition.x += typeWidth;
            contentPosition.width = position.width - typeWidth;

            EditorGUIUtility.labelWidth = labelWidth;

            switch ( typeValue )
            {
                case BossActionControllerBase.ActionType.Wait:
                    EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "DurationParameter" ),
                        new GUIContent( "Duration" ) );
                    break;
                case BossActionControllerBase.ActionType.Move:
                    EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "TargetTypeParameter" ),
                        new GUIContent( "Target" ) );
                    break;
                case BossActionControllerBase.ActionType.Attack:
                    EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "CountParameter" ),
                        new GUIContent( "Count" ) );
                    break;
                case BossActionControllerBase.ActionType.Count:
                case BossActionControllerBase.ActionType.JumpAttack:
                case BossActionControllerBase.ActionType.Charge:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}