using UnityEngine;

public class PassThroughColliders : MonoBehaviour
{
    public float Threshold = 0;

    private Collider2D[] _colliders;
    private Mobile _player;
    private PlayerSettings _playerSettings;

    private Mobile Player
    {
        get
        {
            if ( _player == null )
            {
                var playerObject = GameObject.FindGameObjectWithTag( Tags.Player );
                if ( playerObject != null )
                {
                    _player = playerObject.GetComponent<Mobile>();
                }

                return _player;
            }

            return _player;
        }
    }

    private void Start()
    {
        _colliders = GetComponentsInChildren<Collider2D>();
    }

    private void Update()
    {
        if ( Player == null ) return;

        foreach ( var col in _colliders )
        {
            var threshold = col.enabled
                ? -0.5f * Player.BodySize.y - Threshold
                : 0.5f * Player.BodySize.y + Threshold;

            col.enabled = true; // must be enabled to check bounds
            var toPlayer = Player.BodyPosition - (Vector2)col.bounds.center;

            col.enabled = toPlayer.y > threshold;
        }
    }
}