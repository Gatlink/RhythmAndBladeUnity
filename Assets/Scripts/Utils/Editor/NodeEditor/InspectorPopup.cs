using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    public class InspectorPopup : EditorWindow
    {
        private MonoBehaviour _target;
        
        public static void ShowInspectorPopup( MonoBehaviour behaviour )        
        {
            var window = GetWindow<InspectorPopup>();
            window.titleContent = new GUIContent( behaviour.name + " inspector" );
            window._target = behaviour;
        }

        private void OnGUI()
        {
            var editor = Editor.CreateEditor( _target );
            editor.DrawDefaultInspector();
        }
    }
}