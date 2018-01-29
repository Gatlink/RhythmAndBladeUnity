using UnityEngine;

public class GamepadController
{
	private const float AXIS_DEADZONE = 0.3f;

	private readonly Actor actor;

	public GamepadController(Actor actr)
	{
		actor = actr;
	}

	public void Update ()
	{
		var axis = Input.GetAxis("Horizontal");
		axis = Mathf.Abs(axis) <= AXIS_DEADZONE ? 0f : Mathf.Sign(axis);
		actor.desiredVelocity.x = axis * actor.horizontalMovement.maxSpeed;

		if (Input.GetButtonDown("Jump"))
			actor.Jump();

		if (Input.GetButtonDown("Dash"))
			actor.TransitionTo<StateDash>();
	}
}
