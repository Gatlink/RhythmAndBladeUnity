using Gamelogic.Extensions;
using UnityEngine;

public class PlaceHolderAnimator : MonoBehaviour
{
    private Actor _actor;
    private Transform _muzzle;
    private Vector3 _initialScale;

    private void Start()
    {
        _actor = GetComponentInParent<Actor>();
        _muzzle = transform.Find( "Ball/Muzzle" );
        _initialScale = _muzzle.localScale;
    }

    private void Update()
    {
        _muzzle.localScale = _initialScale.WithX( _initialScale.x * _actor.Direction );
    }
}