using UnityEditor;

[CustomEditor(typeof(Rail))]
public class RailEditor : Editor
{
	private Rail rail;

	public void Awake()
	{
		rail = target as Rail;
	}

	public void OnSceneGUI()
	{
		rail.DrawHandlesEditable();
	}
}