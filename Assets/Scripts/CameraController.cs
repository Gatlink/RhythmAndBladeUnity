using UnityEngine;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
	[HideInInspector]
	public Actor target;
	public Vector2 offset = new Vector2(5f, 5f);
	[Range(0f, 1f)]
	public float directionChangeFactor = 0.1f;

	private float startingZ;
	private Vector3 currentOffset;

	public void Awake()
	{
		startingZ = transform.position.z;
		target = GameObject.FindGameObjectWithTag( "Player" ).GetComponent<Actor>();
	}
	
	public void LateUpdate()
	{
		var desiredOffset = (Vector3) offset;
		desiredOffset.x *= target.Direction;

		currentOffset = directionChangeFactor * desiredOffset + (1f - directionChangeFactor) * currentOffset;

		var newPos = target.transform.position + currentOffset;
		newPos.z = startingZ;
		transform.position = newPos;
	}
}
