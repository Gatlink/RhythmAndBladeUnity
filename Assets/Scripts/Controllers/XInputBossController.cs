using UnityEngine;
using XInputDotNetPure;

namespace Controllers
{
    public class XInputBossController : XInputControllerBase<BossActor>
    {
        // ReSharper disable once Unity.RedundantEventFunction
        // adds "enable" check box in inspector
        private void Start()
        {
        }

        private void ResetIntent( BossActor actor )
        {
            actor.DesiredMovement = 0;
            actor.DesiredAttack = false;
            actor.DesiredJumpAttack = false;
            actor.DesiredJump = false;
            actor.DesiredCharge = false;
        }

        public override void UpdateActorIntent( BossActor actor )
        {
            ResetIntent( actor );
            base.UpdateActorIntent( actor );

            if ( State.Triggers.Left > DeadZone )
            {
                return;
            }

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

            actor.DesiredJump = State.Buttons.A == ButtonState.Pressed &&
                                  PrevState.Buttons.A == ButtonState.Released;

            actor.DesiredJumpAttack = State.Buttons.Y == ButtonState.Pressed &&
                                    PrevState.Buttons.Y == ButtonState.Released;

            actor.DesiredAttack = State.Buttons.X == ButtonState.Pressed;

            actor.DesiredCharge = State.Buttons.B == ButtonState.Pressed &&
                                  PrevState.Buttons.B == ButtonState.Released;
        }
    }
}