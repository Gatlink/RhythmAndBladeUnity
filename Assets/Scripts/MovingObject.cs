using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{	
	public Vector2 Displacement = Vector2.up;
	public float Period = 1f;
	public float Phase = 0f;
	private Vector3 _initialPosition;

	private void Start ()
	{
		_initialPosition = transform.position;
	}

	private void Update ()
	{
		transform.position = Vector3.Lerp(_initialPosition, _initialPosition + (Vector3) Displacement, Mathf.PingPong(Time.time + Phase, Period * 0.5f) / (Period * 0.5f));
	}
}
