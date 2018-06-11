using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions;
using UnityEngine;

public class CheckpointManager : Singleton<CheckpointManager>
{
    private struct Checkpoint
    {
        public Collider2D Collider;
        public bool IsPermanent;

        public Checkpoint( Collider2D collider, bool isPermanent )
        {
            Collider = collider;
            IsPermanent = isPermanent;
        }
    } 
    
    private List<Checkpoint> _checkpoints;
    private Mobile _player;

    private Checkpoint? _lastCheckpoint;
    private float _directionAtCheckpoint;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Mobile>();

        _checkpoints = GameObject.FindGameObjectsWithTag( Tags.Checkpoint )
            .Union( GameObject.FindGameObjectsWithTag( Tags.PermanentCheckpoint ) )
            .Select( go => new Checkpoint(go.GetComponent<Collider2D>(), go.CompareTag( Tags.PermanentCheckpoint ) ) )
            .ToList();
    }

    public static void TeleportPlayerToLastCheckpoint()
    {
        var checkpoint = Instance._lastCheckpoint;
        if ( !checkpoint.HasValue )
        {
            SceneLoader.Instance.ReloadCurrentScene();
        }
        else
        {
            CameraFade.FadeTo( 1, 0.5f, () =>
            {
                var player = Instance._player;
                player.transform.position = checkpoint.Value.Collider.transform.position;
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
            if ( checkpoint.Collider.OverlapPoint( _player.BodyPosition ) )
            {
                _lastCheckpoint = checkpoint;
                _directionAtCheckpoint = _player.Direction;

                if ( checkpoint.IsPermanent )
                {
                    RespawnPoint.Instance.SetRespawn( checkpoint.Collider.transform.position, _directionAtCheckpoint );
                }

                _checkpoints.RemoveAt( i );
                break;
            }
        }
    }
}