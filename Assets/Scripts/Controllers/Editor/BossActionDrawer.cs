using System;
using Controllers;
using UnityEditor;
using UnityEngine;

[ CustomPropertyDrawer( typeof( BossBehaviour.Action ) ) ]
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
            var typeValue = (BossBehaviour.ActionType) typeProp.enumValueIndex;

            if ( typeValue != BossBehaviour.ActionType.Charge )
            {
                contentPosition.width = typeWidth;
            }

            typeValue = (BossBehaviour.ActionType) EditorGUI.EnumPopup( contentPosition, GUIContent.none,
                typeValue );
            typeProp.enumValueIndex = (int) typeValue;

            if ( typeValue == BossBehaviour.ActionType.Charge )
            {
                return;
            }

            contentPosition.x += typeWidth;
            contentPosition.width = position.width - typeWidth;

            EditorGUIUtility.labelWidth = labelWidth;

            switch ( typeValue )
            {
                case BossBehaviour.ActionType.Wait:
                    EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "DurationParameter" ),
                        new GUIContent( "Duration" ) );
                    break;
                case BossBehaviour.ActionType.JumpAttack:
                case BossBehaviour.ActionType.Move:
                    EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "TargetTypeParameter" ),
                        new GUIContent( "Target" ) );
                    break;
                case BossBehaviour.ActionType.Attack:
                    EditorGUI.PropertyField( contentPosition, property.FindPropertyRelative( "CountParameter" ),
                        new GUIContent( "Count" ) );
                    break;
                case BossBehaviour.ActionType.Count:
                case BossBehaviour.ActionType.Charge:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}