using UnityEngine;

public class GamepadController : ActorControllerBase
{
	public override void UpdateActorIntent( Actor actor )
	{
		actor.DesiredMovement = Input.GetAxis("Horizontal");
		actor.DesiredJump = Input.GetButtonDown( "Jump" );
		//actor.DesiredAttack = Input.GetButtonDown( "Fire1" );
		//actor.DesiredDash = Input.GetButtonDown( "Dash" );
	}
}
