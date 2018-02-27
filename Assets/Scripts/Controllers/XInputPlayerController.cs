using Gamelogic.Extensions;
using UnityEngine;
using XInputDotNetPure;

namespace Controllers
{
    public class XInputPlayerController : GLMonoBehaviour, IActorController<PlayerActor>
    {
        private bool _playerIndexSet = false;
        private PlayerIndex _playerIndex;
        private GamePadState _state;
        private GamePadState _prevState;

        private bool HasPlayer
        {
            get { return _playerIndexSet && _prevState.IsConnected; }
        }

        [ Range( 0, 0.9f ) ]
        public float DeadZone = 0.3f;

        public bool Step = true;

        public void UpdateActorIntent( PlayerActor actor )
        {
            _prevState = _state;

            if ( !HasPlayer )
            {
                FindPlayer();
            }

            _state = GamePad.GetState( _playerIndex );

            var rawAxis = _state.ThumbSticks.Left.X;

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

            actor.DesiredJump = _state.Buttons.A == ButtonState.Pressed && _prevState.Buttons.A == ButtonState.Released;
            actor.DesiredDash = _state.Buttons.B == ButtonState.Pressed && _prevState.Buttons.B == ButtonState.Released;
            actor.DesiredAttack =
                _state.Buttons.X == ButtonState.Pressed && _prevState.Buttons.X == ButtonState.Released;
        }

        private void FindPlayer()
        {
            for ( var i = 0; i < 4; ++i )
            {
                var testPlayerIndex = (PlayerIndex) i;
                var testState = GamePad.GetState( testPlayerIndex );
                if ( testState.IsConnected )
                {
                    Debug.Log( string.Format( "XInput GamePad found {0}", testPlayerIndex ) );
                    _playerIndex = testPlayerIndex;
                    _playerIndexSet = true;
                }
            }
        }
    }
}