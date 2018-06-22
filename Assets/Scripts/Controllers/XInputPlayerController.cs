using UnityEngine;
using XInputDotNetPure;

namespace Controllers
{
    public class XInputPlayerController : XInputControllerBase<PlayerActor>
    {
        private void ResetIntent( PlayerActor actor )
        {
            actor.DesiredMovement = 0;
            actor.DesiredJump = false;
            actor.DesiredIgnorePassThrough = false;
            actor.DesiredAttack = false;
            actor.DesiredDash = false;
            actor.DesiredBeatMode = false;
            actor.DesiredBeatActions = BeatManager.BeatAction.None;
        }

        public override void UpdateActorIntent( PlayerActor actor )
        {
            ResetIntent( actor );
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
                // either Jump or Jump Down
                var downPressed = State.ThumbSticks.Left.Y < -DeadZone;
                var jumpPressed = State.Buttons.A == ButtonState.Pressed;
                var jumpJustPressed = jumpPressed && PrevState.Buttons.A == ButtonState.Released;

                actor.DesiredJump = !downPressed && jumpJustPressed;
                actor.DesiredIgnorePassThrough = downPressed && jumpPressed;

                actor.DesiredDash = State.Buttons.B == ButtonState.Pressed &&
                                    PrevState.Buttons.B == ButtonState.Released;
                actor.DesiredAttack =
                    State.Buttons.X == ButtonState.Pressed && PrevState.Buttons.X == ButtonState.Released;
            }
        }
    }
}