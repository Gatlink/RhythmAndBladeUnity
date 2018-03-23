using System;
using System.Collections;
using System.Reflection;
using Controllers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NodeEditor
{
    public class InspectorPopup : EditorWindow
    {
        private Editor _editor;

        public static InspectorPopup ShowInspectorPopup( BossBehaviour behaviour, BehaviourNode node )
        {
            var window = GetWindow<InspectorPopup>();
            window.titleContent = new GUIContent( "Node Editor" );
            window._editor =
                BehaviourNodeEditor.CreateEditorForProperty( behaviour, window.GetNodePath( behaviour, node ) );
            return window;
        }

        private void OnGUI()
        {
//            if ( _editor.target != _target )
//            {
//                Debug.LogWarning( "editor target changed" );
//                DestroyImmediate( _editor );
//                _editor = Editor.CreateEditor( _target );
//            }

            _editor.OnInspectorGUI();
        }

        private void OnEnable()
        {
            autoRepaintOnSceneChange = true;
        }

        private FieldInfo FindFieldInHierarchy( Type type, string name )
        {
            FieldInfo found = null;
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public |
                                       BindingFlags.Instance | BindingFlags.Static |
                                       BindingFlags.DeclaredOnly;
            while ( found == null && type.BaseType != null )
            {
                found = type.GetField( name, flags );
                type = type.BaseType;
            }

            return found;
        }

        private string GetNodePath( BossBehaviour behaviour, BehaviourNode node )
        {
            var arrayName = node is ActionBehaviourNode ? "_actionBehaviours" : "_compoundBehaviours";
            var arrayField = behaviour.GetType().GetField( arrayName, BindingFlags.NonPublic | BindingFlags.Instance );
            if ( arrayField == null )
            {
                throw new ApplicationException( string.Format( "Cannot find field {0} in class {1}", arrayName,
                    behaviour.GetType() ) );
            }

            var dict = arrayField.GetValue( behaviour );
            var keysField = FindFieldInHierarchy( dict.GetType(), "_keys" );
            if ( keysField == null )
            {
                throw new ApplicationException( string.Format( "Cannot find field {0} in class {1}", "_keys",
                    dict.GetType() ) );
            }

            var list = (IList) keysField.GetValue( dict );

            var index = list.IndexOf( node.Guid );
            if ( index < 0 )
            {
                throw new ArgumentException( string.Format( "Cannot find node {0}", node ) );
            }

            var path = string.Format( "{0}._values.Array.data[{1}]", arrayName, index );
            return path;
        }

//
//        private void OnLostFocus()
//        {
//            Close();
//        }
    }

    public class BehaviourNodeEditor : Editor
    {
        private string _targetPropertyPath;

        private Vector2 _scrollPos;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var property = serializedObject.FindProperty( _targetPropertyPath );
            property.isExpanded = true;
            EditorGUILayout.PropertyField( property, true );

//            var prop = serializedObject.GetIterator();
//            using ( var scrollView = new EditorGUILayout.ScrollViewScope( _scrollPos ) )
//            {
//                _scrollPos = scrollView.scrollPosition;
//                while ( prop.Next( true ) )
//                {
//                    EditorGUILayout.LabelField( prop.propertyPath );
//                }
//            }

            serializedObject.ApplyModifiedProperties();
        }

        public static BehaviourNodeEditor CreateEditorForProperty( Object target, string path )
        {
            var editor = CreateEditor( target, typeof( BehaviourNodeEditor ) ) as BehaviourNodeEditor;
            if ( editor == null )
            {
                throw new ArgumentException( string.Format( "Could not create editor of type {0} for target {1}",
                    typeof( BehaviourNodeEditor ), target ) );
            }

            editor._targetPropertyPath = path;
            return editor;
        }

        protected override bool ShouldHideOpenButton()
        {
            return true;
        }
    }
}