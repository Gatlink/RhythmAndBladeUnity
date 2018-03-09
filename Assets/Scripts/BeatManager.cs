using System.Collections;
using Gamelogic.Extensions;
using UnityEngine;

public class BeatManager : Singleton<BeatManager>
{
    public delegate void BeatEventHandler( int beatCount, float nextBeat );

    public event BeatEventHandler BeatEvent;

    public float FirstBeatOffset;

    public float BeatPerMinute;

    public float BeatPeriodSeconds
    {
        get { return 60 / BeatPerMinute; }
    }

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

        yield return new WaitUntil( () => Time >= FirstBeatOffset );

        int beatCount = 0;
        var targetTime = FirstBeatOffset;
        var totalTime = _source.clip.length;
        while ( targetTime <= totalTime )
        {
            targetTime += BeatPeriodSeconds;
            var nextBeat = targetTime - Time;
            OnBeatEvent( beatCount, nextBeat );
            yield return new WaitForSecondsRealtime( nextBeat );
        }
    }

    private void OnBeatEvent( int beatCount, float nextBeat )
    {
        var handler = BeatEvent;
        if ( handler != null ) handler( beatCount, nextBeat );
    }
}