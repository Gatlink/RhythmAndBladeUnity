using UnityEngine;

public class PassThroughColliders : MonoBehaviour
{
    public float Threshold = 0;

    private Collider2D[] _colliders;
    private Transform _player;
    private PlayerSettings _playerSettings;

    private Transform Player
    {
        get
        {
            if ( _player == null )
            {
                var playerObject = GameObject.FindGameObjectWithTag( Tags.Player );
                if ( playerObject != null )
                {
                    _player = playerObject.transform;
                }

                return _player;
            }

            return _player;
        }
    }

    private void Start()
    {
        _colliders = GetComponentsInChildren<Collider2D>();
        _playerSettings = PlayerSettings.Instance;
    }

    private void Update()
    {
        if ( Player == null ) return;

        foreach ( var col in _colliders )
        {
            var threshold = col.enabled
                ? -_playerSettings.BodyRadius - Threshold
                : _playerSettings.BodyRadius + Threshold;

            col.enabled = true; // must be enabled to check bounds
            var toPlayer = Player.position - col.bounds.center;

            col.enabled = toPlayer.y > threshold;
        }
    }
}