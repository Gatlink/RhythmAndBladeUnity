using System.Linq;
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
        GUILayout.Label( string.Format( "{0} nodes ({1} null(s))", bossBehaviour.TotalNodeCount,
            bossBehaviour.GetAllBehaviourNodes().Sum( c => c == null ? 1 : 0 ) ) );
        if ( GUILayout.Button( "View in Editor" ) )
        {
            NodeBasedEditor.EditBossBehaviour( bossBehaviour );
        }
        
        base.OnInspectorGUI();
    }
}