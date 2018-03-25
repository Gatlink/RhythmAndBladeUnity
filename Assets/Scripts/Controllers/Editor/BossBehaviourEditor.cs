using Controllers;
using NodeEditor;
using UnityEditor;
using UnityEngine;

[ CustomEditor( typeof( BossBehaviour ) ) ]
public class BossBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.ObjectField( "Script:", MonoScript.FromScriptableObject( (BossBehaviour) target ),
            typeof( BossBehaviour ), false );
        GUI.enabled = true;

        var bossBehaviour = (BossBehaviour) target;
        GUILayout.Label( string.Format( "{0} node(s)", bossBehaviour.TotalNodeCount ) );
        if ( GUILayout.Button( "View in Editor" ) )
        {
            NodeBasedEditor.EditBossBehaviour( bossBehaviour );
        }
    }
}