using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ CustomPropertyDrawer( typeof( AudioTrack.BeatList ) ) ]
public class BeatEventListDrawer : PropertyDrawer
{
    private AudioTrack.BeatList _instance;
    private float _offset;
    private float _bpm;
    private float _duration;

    private AudioTrack.BeatList GetPropertyObject( SerializedProperty property )
    {
        if ( _instance == null )
        {
            var path = property.propertyPath.Split( '.' );
            object propertyObject = property.serializedObject.targetObject;
            foreach ( var pathNode in path )
            {
                propertyObject = propertyObject.GetType().GetField( pathNode ).GetValue( propertyObject );
            }

            _instance = propertyObject as AudioTrack.BeatList;
        }

        return _instance;
    }

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        using ( var scope = new EditorGUI.PropertyScope( position, label, property ) )
        {
            var target = GetPropertyObject( property );

            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PrefixLabel( position, scope.content );

            var contentPosition = position;
            EditorGUI.indentLevel = 1;
            contentPosition = EditorGUI.IndentedRect( contentPosition );
            EditorGUI.indentLevel = 0;
            contentPosition.y += contentPosition.height + 1;

            var infos = string.Format( "Current: {0} beats loaded.", target.BeatCount );
            var preview = string.Join( "\n",
                target.Take( 5 ).Select( s => TimeSpan.FromSeconds( s ).ToString() ).ToArray() );
            EditorGUI.LabelField( contentPosition, new GUIContent( infos, preview + "\n..." ) );

            contentPosition.y += contentPosition.height + 1;
            if ( GUI.Button( contentPosition, "Set From File" ) )
            {
                var path = EditorUtility.OpenFilePanel( "Load beat list", ".", "*" );
                if ( !string.IsNullOrEmpty( path ) && File.Exists( path ) )
                {
                    LoadFromText( target, File.ReadAllLines( path ) );
                }
            }

            const int buttonWidth = 64;
            contentPosition.y += contentPosition.height + 1;
            contentPosition.width -= buttonWidth;
            contentPosition.width /= 3;
            EditorGUIUtility.labelWidth = 32;

            _bpm = EditorGUI.FloatField( contentPosition, new GUIContent( "BPM", "Beats per minute" ), _bpm );
            contentPosition.x += contentPosition.width;
            _duration = EditorGUI.FloatField( contentPosition, new GUIContent( "Dur.", "Total duration in seconds" ),
                _duration );
            contentPosition.x += contentPosition.width;
            _offset = EditorGUI.FloatField( contentPosition, new GUIContent( "Off.", "Start offset in seconds" ),
                _offset );
            contentPosition.x += contentPosition.width;
            contentPosition.width = buttonWidth;
            if ( GUI.Button( contentPosition, "Set" ) )
            {
                if ( _duration == 0 )
                {
                    EditorUtility.DisplayDialog( "Empty", "Beats parameters not set (duration is 0)", "OK" );
                }
                else
                {
                    target.SetFromInterval( 60 / _bpm, _duration, _offset );
                }
            }
        }
    }

    private struct NumberedLine
    {
        public readonly int Num;
        public readonly string Text;

        public NumberedLine( int num, string text )
        {
            Num = num;
            Text = text;
        }
    }

    private static void LoadFromText( AudioTrack.BeatList target, IEnumerable<string> lines )
    {
        var beats = lines.Select( ( l, i ) => new NumberedLine( i, l.Trim() ) )
            .Where( l => !l.Text.StartsWith( "#" ) )
            .Select( l =>
            {
                TimeSpan val;
                if ( !TimeSpan.TryParse( l.Text, out val ) )
                {
                    var errorMessage = string.Format( "Parse error at line {0}: {1}", l.Num, l.Text );
                    EditorUtility.DisplayDialog( "Error", errorMessage, "OK" );
                    throw new ArgumentException( errorMessage );
                }

                return (float) val.TotalSeconds;
            } );
        target.SetFromBeats( beats );
    }

    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        return 4 * base.GetPropertyHeight( property, label ) + 3;
    }
}