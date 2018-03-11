using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ CreateAssetMenu ]
public class AudioTrack : ScriptableObject
{
    public AudioClip Clip;
    public BeatList Beats;

    [ Serializable ]
    public class BeatList : IEnumerable<float>
    {
        [ SerializeField ]
        [ HideInInspector ]
        private float[] _beats;

        public BeatList( float interval, float duration, float offset = 0 )
        {
            SetFromInterval( interval, duration, offset );
        }

        public BeatList( IEnumerable<float> beats )
        {
            SetFromBeats( beats );
        }

        public int BeatCount { get; private set; }

        public void SetFromBeats( IEnumerable<float> beats )
        {
            _beats = beats.ToArray();
            BeatCount = _beats.Length;
        }

        public void SetFromInterval( float interval, float duration, float offset )
        {
            var count = Mathf.CeilToInt( ( duration - offset ) / interval );
            _beats = new float[ count ];
            var beat = offset;
            for ( var i = 0; i < count; i++ )
            {
                _beats[ i ] = beat;
                beat += interval;
            }

            BeatCount = count;
        }

        public IEnumerator<float> GetEnumerator()
        {
            return _beats.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}