using UnityEngine;

public class HurtPlayer : MonoBehaviour, IHarmfull
{
    [ SerializeField ]
    private int _damage = 1;

    [ SerializeField ]
    private float _recoil = 1;

    [ SerializeField ]
    private bool _passiveHurt;

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

    public bool PassiveHurt
    {
        get { return _passiveHurt; }
    }
}