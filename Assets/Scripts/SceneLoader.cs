using Gamelogic.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    private bool _sceneLoadPending;
    private PlayerActor _player;
    private LayerMask _levelTriggerLayer;

    private RespawnPoint _respawnPoint;

    private void Start()
    {
        _levelTriggerLayer = 1 << LayerMask.NameToLayer( Layers.LevelTrigger );
        _player = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<PlayerActor>();

        _respawnPoint = RespawnPoint.Instance;
        if ( !_respawnPoint.HasSpawnInfo )
        {
            _respawnPoint.SetRespawn( _player.transform.position, _player.Mobile.Direction );
        }
        else
        {
            _player.transform.position = _respawnPoint.Position;
            _player.Mobile.Direction = _respawnPoint.Direction;
        }
    }

    private void LateUpdate()
    {
        if ( _sceneLoadPending ) return;

        if ( !_player.Health.IsAlive )
        {
            ReloadCurrentScene();
        }

        if ( Physics2D.OverlapPoint( _player.transform.position, _levelTriggerLayer ) != null )
        {
            // load next scene
            Debug.Log( "Destroying respawn" );
            Destroy( _respawnPoint );

            LoadScene( SceneManager.GetActiveScene().buildIndex + 1 );
        }
    }

    private void LoadScene( int buildIndex )
    {
        _sceneLoadPending = true;
        CameraFade.FadeTo( 1, 0.5f, () => SceneManager.LoadScene( buildIndex ), false );
    }

    public void ReloadCurrentScene()
    {
        LoadScene( SceneManager.GetActiveScene().buildIndex );
    }
}