using UnityEngine;

public class ShakeOnHit : MonoBehaviour
{
	private void OnEnable()
	{
		var destroyable = GetComponentInParent<DestroyWhenHit>();
		if ( destroyable != null )
		{
			destroyable.HitEvent += OnHit;
		}
	}

	private void OnDisable()
	{
		var destroyable = GetComponentInParent<DestroyWhenHit>();
		if ( destroyable != null )
		{
			destroyable.HitEvent -= OnHit;
		}
	}

	private void OnHit()
	{
		var shaker = GetComponent<ShakeBehaviour>();
		shaker.Shake(1, 0.5f);
	}
}
