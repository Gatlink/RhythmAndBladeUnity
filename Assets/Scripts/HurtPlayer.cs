using UnityEngine;

public class HurtPlayer : MonoBehaviour, IHarmfull
{
    [ SerializeField ]
    private int _damage = 1;

    [ SerializeField ]
    private float _recoil = 1;

    [ SerializeField ]
    private bool _passiveHurt;

    [ SerializeField ]
    private bool _teleportToLastCheckpoint;

    public int Damage
    {
        get { return _damage; }
        set { _damage = value; }
    }

    public float Recoil
    {
        get { return _recoil; }
        set { _recoil = value; }
    }

    public GameObject GameObject
    {
        get { return gameObject; }
    }

    public bool SkipHurtState
    {
        get { return _passiveHurt; }
    }

    public bool TeleportToLastCheckpoint
    {
        get { return _teleportToLastCheckpoint; }
    }
}