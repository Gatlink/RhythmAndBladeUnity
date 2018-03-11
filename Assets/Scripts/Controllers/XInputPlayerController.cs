using UnityEngine;
using XInputDotNetPure;

namespace Controllers
{
    public class XInputPlayerController : XInputControllerBase<PlayerActor>
    {
        public override void UpdateActorIntent( PlayerActor actor )
        {
            base.UpdateActorIntent( actor );

            var rawAxis = State.ThumbSticks.Left.X;

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
            
            if ( State.Triggers.Right > DeadZone )
            {
                // beat mode
                actor.DesiredBeatMode = true;

                if ( State.Buttons.X == ButtonState.Pressed )
                {
                    actor.DesiredBeatActions |= BeatManager.BeatAction.Left;
                }
                if ( State.Buttons.Y == ButtonState.Pressed )
                {
                    actor.DesiredBeatActions |= BeatManager.BeatAction.Up;
                }
                if ( State.Buttons.A == ButtonState.Pressed )
                {
                    actor.DesiredBeatActions |= BeatManager.BeatAction.Down;
                }
                if ( State.Buttons.B == ButtonState.Pressed )
                {
                    actor.DesiredBeatActions |= BeatManager.BeatAction.Right;
                }
            }
            else
            {
                actor.DesiredJump = State.Buttons.A == ButtonState.Pressed &&
                                    PrevState.Buttons.A == ButtonState.Released;
                actor.DesiredDash = State.Buttons.B == ButtonState.Pressed &&
                                    PrevState.Buttons.B == ButtonState.Released;
                actor.DesiredAttack =
                    State.Buttons.X == ButtonState.Pressed && PrevState.Buttons.X == ButtonState.Released;
            }
        }
    }
}