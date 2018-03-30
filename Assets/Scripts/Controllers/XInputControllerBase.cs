using System;
using Gamelogic.Extensions;
using UnityEngine;
using XInputDotNetPure;

namespace Controllers
{
    public abstract class XInputControllerBase<TActor> : GLMonoBehaviour, IActorController<TActor>
    {
        public bool AutoFindPlayer;
        public PlayerIndex PlayerIndex;
        private bool _playerIndexSet;
        protected GamePadState State;
        protected GamePadState PrevState;

        [ Range( 0, 0.9f ) ]
        public float DeadZone = 0.3f;

        public bool Step = true;

        public bool HasPlayer
        {
            get { return _playerIndexSet && PrevState.IsConnected; }
        }

        private void Start()
        {
            try
            {
                GamePad.GetState( PlayerIndex.One );
            }
            catch ( DllNotFoundException )
            {
                Destroy( this );
            }
        }

        private void FindPlayer()
        {
            var from = (int) PlayerIndex;
            for ( var i = 0; i < 4; ++i )
            {
                var testPlayerIndex = (PlayerIndex) ( ( from + i ) % 4 );
                var testState = GamePad.GetState( testPlayerIndex );
                if ( testState.IsConnected )
                {
                    Debug.Log( string.Format( "XInput GamePad found {0}", testPlayerIndex ) );
                    PlayerIndex = testPlayerIndex;
                    _playerIndexSet = true;
                }
            }
        }

        public bool Enabled
        {
            get { return enabled; }
        }

        public virtual void UpdateActorIntent( TActor actor )
        {
            PrevState = State;

            if ( !HasPlayer && AutoFindPlayer )
            {
                FindPlayer();
            }

            if ( !HasPlayer )
            {
                enabled = false;
            }

            State = GamePad.GetState( PlayerIndex );
        }
    }
}