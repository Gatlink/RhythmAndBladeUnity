using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions;
using UnityEngine;

public class BeatManager : Singleton<BeatManager>
{
    public delegate void NextBeatEventHandler( int beatCount, float nextBeat );

    public event NextBeatEventHandler NextBeatEvent;

    public BeatEventList Beats;

    public float Time
    {
        get { return _source.time; }
    }

    private AudioSource _source;

    private void Awake()
    {
        _source = GetRequiredComponent<AudioSource>();
    }

    private void OnEnable()
    {
        StartCoroutine( PlayCoroutine() );
    }

    private void OnDisable()
    {
        _source.Stop();
        StopAllCoroutines();
    }

    private IEnumerator PlayCoroutine()
    {
        _source.Play();

        var beatCount = 0;

        foreach ( var beatTime in Beats )
        {
            var nextBeatDuration = beatTime - Time;
            OnNextBeatEvent( beatCount++, nextBeatDuration );
            yield return new WaitForSecondsRealtime( nextBeatDuration );
        }
    }

    private void OnNextBeatEvent( int beatCount, float nextBeat )
    {
        var handler = NextBeatEvent;
        if ( handler != null ) handler( beatCount, nextBeat );
    }

    [ Serializable ]
    public class BeatEventList : IEnumerable<float>
    {
        [ SerializeField ]
        [ HideInInspector ]
        private float[] _beats;

        public BeatEventList( float interval, float duration, float offset = 0 )
        {
            SetFromInterval( interval, duration, offset );
        }

        public BeatEventList( IEnumerable<float> beats )
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