using UnityEngine;
using XInputDotNetPure;

namespace Controllers
{
    public class XInputBossController : XInputControllerBase<BossActor>
    {
        public override void UpdateActorIntent( BossActor actor )
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

            actor.DesiredJumpAttack = State.Buttons.A == ButtonState.Pressed &&
                                      PrevState.Buttons.A == ButtonState.Released;

            actor.DesiredAttack = State.Buttons.X == ButtonState.Pressed;

            actor.DesiredCharge = State.Buttons.B == ButtonState.Pressed &&
                                  PrevState.Buttons.B == ButtonState.Released;
        }
    }
}