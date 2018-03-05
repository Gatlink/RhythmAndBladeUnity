using UnityEngine;

public class HurtPlayer : MonoBehaviour, IHarmfull
{
    [ SerializeField ]
    private int _damage;

    [SerializeField]
    private float _recoil;

    public int Damage
    {
        get { return _damage; }
    }

    public float Recoil
    {
        get { return _recoil; }
    }

    public GameObject GameObject
    {
        get { return gameObject; }
    }
}