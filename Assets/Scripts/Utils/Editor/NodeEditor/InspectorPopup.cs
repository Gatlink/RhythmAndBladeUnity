﻿using Controllers;
using UnityEditor;
using UnityEngine;

namespace NodeEditor
{
    public class InspectorPopup : EditorWindow
    {
        private Object _target;
        private Editor _editor;

        public static InspectorPopup ShowInspectorPopup( BossBehaviour behaviour, BehaviourNode node )
        {
            var window = GetWindow<InspectorPopup>();
            window.titleContent = new GUIContent( behaviour.name + " inspector" );
            window._target = behaviour;
            window._editor = Editor.CreateEditor( behaviour );
            return window;
        }

        private void OnGUI()
        {
            if ( _editor.target != _target )
            {
                Debug.LogWarning( "editor target changed" );
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