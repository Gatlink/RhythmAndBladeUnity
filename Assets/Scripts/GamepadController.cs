using Gamelogic.Extensions;
using UnityEngine;

public class GamepadController : ActorControllerBase
{
    [ Range( 0, 0.9f ) ]
    public float DeadZone = 0.3f;

    public bool Step = true;

    public OptionalFloat DebugAxis;
    
    public override void UpdateActorIntent( Actor actor )
    {
        var rawAxis = Input.GetAxis( "Horizontal" );
        if ( DebugAxis.UseValue )
        {
            rawAxis = DebugAxis.Value;
        }

        var amplitude = Mathf.Abs( rawAxis );
        var direction = Mathf.Sign( rawAxis );
        
        if ( amplitude < DeadZone )
        {
            amplitude = 0;
        }
        else
        {
            amplitude = ( amplitude - DeadZone ) / ( 1 - DeadZone );
        }

        if ( Step && amplitude > 0 )
        {
            amplitude = 1;
        }        
        
        actor.DesiredMovement = direction * amplitude;        
        actor.DesiredJump = Input.GetButtonDown( "Jump" );
        //actor.DesiredAttack = Input.GetButtonDown( "Fire1" );
        //actor.DesiredDash = Input.GetButtonDown( "Dash" );
    }
}