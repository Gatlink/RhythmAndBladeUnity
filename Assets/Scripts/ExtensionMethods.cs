using UnityEngine;

public static class ExtensionMethods
{
	public static float Slope(this Vector3 self, Vector3 oth)
	{
		var A = self.x < oth.x ? oth : self;
		var B = self.x < oth.x ? self : oth;

		var AB = B - A;
		var angle = (Vector3.Angle(Vector3.left, AB) + 360) % 360;

		return angle > 90 ? 360 - angle : angle;
	}
}
