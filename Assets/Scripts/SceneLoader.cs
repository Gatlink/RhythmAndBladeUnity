using Gamelogic.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    private bool _sceneLoadPending;
    private PlayerActor _player;
    private LayerMask _levelTriggerLayer;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<PlayerActor>();
        _levelTriggerLayer = 1 << LayerMask.NameToLayer( Layers.LevelTrigger );
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