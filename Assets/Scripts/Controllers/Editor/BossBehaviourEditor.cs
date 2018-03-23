using Controllers;
using NodeEditor;
using UnityEditor;
using UnityEngine;

[ CustomEditor( typeof( BossBehaviour ) ) ]
public class BossBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var bossBehaviour = (BossBehaviour) target;
        GUILayout.Label( string.Format( "{0} node(s)", bossBehaviour.TotalNodeCount ) );
        if ( GUILayout.Button( "View in Editor" ) )
        {
            NodeBasedEditor.EditBossBehaviour( bossBehaviour );
        }
    }
}