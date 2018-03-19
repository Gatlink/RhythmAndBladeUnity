using Gamelogic.Extensions;
using UnityEngine;

namespace Controllers
{
    public class GamepadPlayerController : GLMonoBehaviour, IActorController<PlayerActor>
    {
        [ Range( 0, 0.9f ) ]
        public float DeadZone = 0.3f;

        public bool Step = true;

        public OptionalFloat DebugAxis;

        public bool Enabled
        {
            get { return enabled; }
        }

        private void ResetIntent( PlayerActor actor )
        {
            actor.DesiredMovement = 0;
            actor.DesiredJump = false;
            actor.DesiredAttack = false;
            actor.DesiredDash = false;
            actor.DesiredBeatMode = false;
            actor.DesiredBeatActions = BeatManager.BeatAction.None;
        }

        public void UpdateActorIntent( PlayerActor actor )
        {
            ResetIntent( actor );
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
            actor.DesiredDash = Input.GetButtonDown( "Dash" );
            actor.DesiredAttack = Input.GetButtonDown( "Fire1" );
        }
    }
}