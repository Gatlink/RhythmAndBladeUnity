using UnityEngine;

public class DestroyWhenHit : MonoBehaviour, IDestructible
{
    public Transform Root;

    public void Hit( GameObject source )
    {
        var target = Root;
        if ( target == null )
        {
            target = transform;
        }

        Destroy( target.gameObject );
    }
}