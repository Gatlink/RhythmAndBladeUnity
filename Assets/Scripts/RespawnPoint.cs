using UnityEngine;

public class RespawnPoint : ScriptableObject
{
    private static RespawnPoint _instance;

    public static RespawnPoint Instance
    {
        get
        {
            if ( _instance == null )
            {
                _instance = CreateInstance<RespawnPoint>();
            }

            return _instance;
        }
    }

    public Vector3 Position { get; private set; }
    public float Direction { get; private set; }
    public bool HasSpawnInfo { get; private set; }

    private void OnDestroy()
    {
        if ( _instance == this )
        {
            _instance = null;
        }
    }

    public void SetRespawn( Vector3 position, float direction )
    {
        Position = position;
        Direction = direction;
        HasSpawnInfo = true;
    }
}