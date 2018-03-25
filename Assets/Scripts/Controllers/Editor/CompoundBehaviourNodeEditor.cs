using Controllers;
using UnityEditor;
using UnityEngine;

[ CustomPropertyDrawer( typeof( CompoundBehaviourNode ) ) ]
public class CompoundBehaviourNodeEditor : PropertyDrawer
{
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        using ( new EditorGUI.PropertyScope( position, GUIContent.none, property ) )
        {
            position.height = EditorGUIUtility.singleLineHeight;

            var nameProp = property.FindPropertyRelative( "Name" );
            nameProp.stringValue = EditorGUI.TextField( position, "Name", nameProp.stringValue );

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            position.width /= 2;

            var randomizeProp = property.FindPropertyRelative( "Randomize" );
            var randomize = randomizeProp.boolValue =
                EditorGUI.Toggle( position, "Randomize", randomizeProp.boolValue );

            position.x += position.width;
            var loopProp = property.FindPropertyRelative( "LoopRepeat" );
            loopProp.boolValue = EditorGUI.Toggle( position, "LoopRepeat", loopProp.boolValue );
            position.x -= position.width;

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            var endCheckProp = property.FindPropertyRelative( "UseHealthEndCondition" );
            var endCheck = endCheckProp.boolValue =
                EditorGUI.Toggle( position, "UseHealthEndCondition", endCheckProp.boolValue );
            if ( endCheck )
            {
                position.x += position.width;
                var healthLimitProp = property.FindPropertyRelative( "HealthEndConditionLimit" );
                healthLimitProp.intValue = EditorGUI.IntField( position, "Health Limit", healthLimitProp.intValue );
                position.x -= position.width;
            }

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            position.width *= 2;

            if ( randomize )
            {
                EditorGUI.LabelField( position, "Sampling Weights:" );
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.indentLevel++;
                var multiplicatorsProp = property.FindPropertyRelative( "_childMultiplicators" );
                var childNodesProp = property.FindPropertyRelative( "_childNodes" );
                var count = multiplicatorsProp.arraySize = childNodesProp.arraySize;
                var node = (BossBehaviour) property.serializedObject.targetObject;
                for ( var i = 0; i < count; i++ )
                {
                    var guid = childNodesProp.GetArrayElementAtIndex( i ).stringValue;
                    var itemName = node.GetBehaviourNode( guid ).Name;
                    var itemProp = multiplicatorsProp.GetArrayElementAtIndex( i );

                    IntFieldWithButtons( position, itemProp, new GUIContent( itemName ) );

                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                EditorGUI.indentLevel--;
            }
        }
    }

    private void IntFieldWithButtons( Rect position, SerializedProperty prop, GUIContent label )
    {
        const int buttonWidth = 32;
        const int spacing = 2;

        position = EditorGUI.PrefixLabel( position, new GUIContent( label ) );
        position.width -= buttonWidth * 2 + spacing * 2;

        var value = prop.intValue;
        value = Mathf.Max( 1, EditorGUI.IntField( position, value ) );
        position.x += position.width + spacing;
        position.width = buttonWidth;
        if ( GUI.Button( position, "-" ) )
        {
            value = Mathf.Max( 1, value - 1 );
        }

        position.x += buttonWidth + spacing;
        if ( GUI.Button( position, "+" ) )
        {
            value++;
        }

        prop.intValue = value;
    }
}