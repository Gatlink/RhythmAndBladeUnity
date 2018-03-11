using System;
using System.Collections;
using System.Collections.Generic;
using Gamelogic.Extensions;
using Gamelogic.Extensions.Algorithms;
using UnityEngine;

public class BeatManager : Singleton<BeatManager>
{
    public delegate void BeatEventHandler( int beatCount, bool playerSuccess, float nextBeat,
        BeatAction nextBeatAction );

    public event BeatEventHandler BeatEvent;

    public AudioTrack Track;

    private IGenerator<BeatAction> _beatActionsGenerator;

    private AudioSource _source;

    private PlayerActor _player;

    public float Time
    {
        get { return _source.time; }
    }

    private void Awake()
    {
        _source = GetRequiredComponent<AudioSource>();
        _beatActionsGenerator = new RandomBagGenerator<BeatAction>(new BeatAction[]
        {
            BeatAction.Up, BeatAction.Down, BeatAction.Left, BeatAction.Right
        });
    }            

    private IEnumerator Start()
    {
        _player = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<PlayerActor>();
        yield return new WaitForEndOfFrame();
        StartCoroutine( PlayCoroutine() );
    }

    private void OnDisable()
    {
        _source.Stop();
        StopAllCoroutines();
    }

    private IEnumerator PlayCoroutine()
    {
        _source.clip = Track.Clip;
        _source.Play();

        yield return new WaitUntil( () => Time > 0 );
        
        var beatCount = 0;
        var playerSuccess = true;
        foreach ( var beatTime in Track.Beats )
        {
            var nextBeatDuration = beatTime - Time;
            var nextBeatAction = _beatActionsGenerator.Next();
            Debug.Log( "Next beat in " + nextBeatDuration );

            OnNextBeatEvent( beatCount++, playerSuccess, nextBeatDuration, nextBeatAction );
            yield return new WaitForSecondsRealtime( nextBeatDuration );
            // check player success
            playerSuccess = _player.DesiredBeatActions == nextBeatAction;
        }
    }

    private void OnNextBeatEvent( int beatCount, bool playerSuccess, float nextBeat, BeatAction nextBeatAction )
    {
        var handler = BeatEvent;
        if ( handler != null ) handler( beatCount, playerSuccess, nextBeat, nextBeatAction );
    }

    [ Flags ]
    public enum BeatAction
    {
        None = 0,
        Up = 1,
        Right = 2,
        Down = 4,
        Left = 8
    }
}