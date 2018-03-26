using Gamelogic.Extensions.Internal;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Gamelogic.Extensions.Editor
{
    /// <summary>
    /// A property drawer for type InspectorList.
    /// </summary>
    [ Version( 2, 5 ) ]
    [ CustomPropertyDrawer( typeof( InspectorList ), true ) ]
    public class InspectorListPropertyDrawer : PropertyDrawer
    {
        private ReorderableList reorderableList;
        private float lastHeight = 50;

        public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
        {
            var list = property.FindPropertyRelative( "values" );

            if ( list == null )
            {
                return 0;
            }

            InitList( list, property );

            if ( reorderableList != null )
            {
                return reorderableList.GetHeight();
            }

            return lastHeight;
        }

        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
        {
            property.serializedObject.Update();
            
            var list = property.FindPropertyRelative( "values" );

            if ( list == null )
            {
                return;
            }

            var indentLevel = EditorGUI.indentLevel;

            InitList( list, property );

            if ( list.arraySize > 0 )
                reorderableList.elementHeight = EditorGUI.GetPropertyHeight( list.GetArrayElementAtIndex( 0 ) );

            if ( position.height <= 0 )
            {
                return;
            }

            lastHeight = reorderableList.GetHeight();

            reorderableList.DoList( position );

            EditorGUI.indentLevel = indentLevel;
            
            property.serializedObject.ApplyModifiedProperties();
        }

        public void InitList( SerializedProperty list, SerializedProperty property )
        {
            if ( reorderableList != null && reorderableList.serializedProperty.serializedObject == list.serializedObject )
            {
                return;
            }
            reorderableList = new ReorderableList( property.serializedObject, list, true, true, true, true )
            {
                drawElementCallback =
                    ( rect, index, isActive, isFocused ) =>
                    {
                        var element = list.GetArrayElementAtIndex( index );
                        if ( element.propertyType == SerializedPropertyType.ObjectReference )
                        {
                            var itemLabel = new GUIContent( "Element: " + index );
                            var obj = element.objectReferenceValue;
                            if ( obj != null )
                            {
                                var prop = obj.GetType().GetField( "Name" );

                                if ( prop != null )
                                {
                                    itemLabel = new GUIContent( (string) prop.GetValue( obj ) );
                                }
                            }

                            EditorGUI.PropertyField( rect, list.GetArrayElementAtIndex( index ), itemLabel, true );
                        }
                        else
                        {
                            var labelProperty = element;
                            var potentialProperty = (SerializedProperty) null;
                            var maxCheck = 0;

                            while ( labelProperty.Next( true ) && maxCheck++ < 3 )
                            {
                                if ( labelProperty.propertyType == SerializedPropertyType.String )
                                {
                                    //TODO: @omar this is always true

                                    if ( labelProperty.name == "name" || potentialProperty == null )
                                    {
                                        potentialProperty = labelProperty;
                                        break;
                                    }
                                }
                            }

                            var itemLabel = potentialProperty == null
                                ? new GUIContent( "Element: " + index )
                                : new GUIContent( labelProperty.stringValue );

                            EditorGUI.PropertyField( rect, list.GetArrayElementAtIndex( index ), itemLabel, true );
                        }
                    },

                drawHeaderCallback =
                    rect =>
                    {
                        EditorGUI.indentLevel++;
                        EditorGUI.LabelField( rect, property.displayName );
                    },


#if UNITY_5
					elementHeightCallback = index => EditorGUI.GetPropertyHeight(list.GetArrayElementAtIndex(index), null, true)
#endif
            };
        }
    }
}