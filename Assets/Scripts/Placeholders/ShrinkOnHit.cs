using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkOnHit : MonoBehaviour
{
	public float Factor = 0.5f;
	
	private Transform _parent;

	private void OnEnable()
	{
		var destroyable = GetComponentInParent<DestroyWhenHit>();
		if ( destroyable != null )
		{
			_parent = destroyable.transform;
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
		var scale = _parent.localScale;
		scale.x = Factor * scale.x;
		_parent.localScale = scale;
	}
}
