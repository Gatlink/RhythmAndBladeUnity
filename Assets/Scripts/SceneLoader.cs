using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private bool _sceneLoadPending;
    private Actor _player;
    private LayerMask _levelTriggerLayer;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Actor>();
        _levelTriggerLayer = 1 << LayerMask.NameToLayer( Layers.LevelTrigger );
    }

    private void LateUpdate()
    {
        if ( _sceneLoadPending ) return;

        if ( _player.CurrentHitCount == 0 )
        {
            // reload current scene
            LoadScene( SceneManager.GetActiveScene().buildIndex );
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
        CameraFade.FadeTo( 1, 0.5f, () => SceneManager.LoadScene( buildIndex ) );
    }
}