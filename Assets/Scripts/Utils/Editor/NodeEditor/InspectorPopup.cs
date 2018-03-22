using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    public class InspectorPopup : EditorWindow
    {
        private MonoBehaviour _target;
        private Editor _editor;

        public static InspectorPopup ShowInspectorPopup( MonoBehaviour behaviour )
        {
            var window = GetWindow<InspectorPopup>();
            window.titleContent = new GUIContent( behaviour.name + " inspector" );
            window._target = behaviour;
            window._editor = Editor.CreateEditor( behaviour );
            Debug.Log( "Show " + behaviour + " " + window._editor, behaviour );
            return window;
        }

        private void OnGUI()
        {
            if ( _editor.target != _target )
            {
                Debug.Log( "editor target changed" );
                DestroyImmediate( _editor );
                _editor = Editor.CreateEditor( _target );
            }

            _editor.DrawDefaultInspector();
        }

        private void OnEnable()
        {
            autoRepaintOnSceneChange = true;
        }

//
//        private void OnLostFocus()
//        {
//            Close();
//        }
    }
}