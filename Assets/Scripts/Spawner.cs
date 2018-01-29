using UnityEngine;

public class Spawner : MonoBehaviour
{
	public GameObject toSpawn;

	public void Start()
	{
		if (toSpawn != null)
			GameObject.Instantiate(toSpawn, transform.position, Quaternion.identity);
	}
}
