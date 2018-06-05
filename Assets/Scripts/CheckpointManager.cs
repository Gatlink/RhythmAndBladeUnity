using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions;
using UnityEngine;

public class CheckpointManager : Singleton<CheckpointManager>
{
    private List<Collider2D> _checkpoints;
    private Mobile _player;

    private Collider2D _lastCheckpoint;
    private float _directionAtCheckpoint;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Mobile>();

        _checkpoints = GameObject.FindGameObjectsWithTag( Tags.Checkpoint )
            .Union( GameObject.FindGameObjectsWithTag( Tags.PermanentCheckpoint ) )
            .Select( go => go.GetComponent<Collider2D>() ).ToList();
    }

    public static void TeleportPlayerToLastCheckpoint()
    {
        var checkpoint = Instance._lastCheckpoint;
        if ( checkpoint == null )
        {
            SceneLoader.Instance.ReloadCurrentScene();
        }
        else
        {
            CameraFade.FadeTo( 1, 0.5f, () =>
            {
                var player = Instance._player;
                player.transform.position = checkpoint.transform.position;
                player.GetComponent<PlayerActor>().RestartToIinitialState();
                player.UpdateDirection( Instance._directionAtCheckpoint );
                CameraFade.FadeFrom( 1, 0.5f, null, false );
            }, false );
        }
    }

    private void Update()
    {
        for ( var i = _checkpoints.Count - 1; i >= 0; i-- )
        {
            var checkpoint = _checkpoints[ i ];
            if ( checkpoint.OverlapPoint( _player.BodyPosition ) )
            {
                _lastCheckpoint = checkpoint;
                _directionAtCheckpoint = _player.Direction;

                RespawnPoint.Instance.SetRespawn( _lastCheckpoint.transform.position, _directionAtCheckpoint );

                _checkpoints.RemoveAt( i );
                break;
            }
        }
    }
}