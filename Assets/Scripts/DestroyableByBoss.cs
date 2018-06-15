using ActorStates.Boss;
using UnityEngine;

public class DestroyableByBoss : MonoBehaviour 
{
	private Collider2D _collider;
	private BossActor _boss;
	private readonly Collider2D[] _collidersBuffer = new Collider2D[1];
	private ContactFilter2D _contactFilter2D;

	private void Start ()
	{
		_collider = GetComponentInChildren<Collider2D>();
		_boss = GameObject.FindGameObjectWithTag( Tags.Boss ).GetComponent<BossActor>();
		_contactFilter2D = new ContactFilter2D();
		_contactFilter2D.SetLayerMask( 1 << LayerMask.NameToLayer( Layers.Harmfull ) );
	}

	private void Update()
	{
		if ( _boss.CurrentState is DiveState && Physics2D.OverlapCollider( _collider, _contactFilter2D, _collidersBuffer ) > 0 )
		{
			// todo fx, etc...
			SlowMotionFx.Freeze();
			Destroy( gameObject );
		}
	}
}
