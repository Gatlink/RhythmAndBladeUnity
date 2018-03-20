using System;
using Gamelogic.Extensions;
using Gamelogic.Extensions.Algorithms;

namespace Controllers
{
    public class BossControllerManager : BossControllerBase
    {
        public bool Randomize;
        public bool LoopRepeat;

        public ControllerList SubControllers;

        private int _nextControllerIndex;
        private BossControllerBase _currentController;

        private void OnEnable()
        {
            Restart();
        }

        private void Restart()
        {
            _nextControllerIndex = 0;
            _currentController = null;
            if ( Randomize )
            {
                SubControllers.Shuffle();
            }
        }

        public override void UpdateActorIntent( BossActor actor )
        {
            base.UpdateActorIntent( actor );
            if ( !Enabled ) return;

            if ( _currentController == null || _currentController.Enabled == false )
            {
                if ( _nextControllerIndex == SubControllers.Count )
                {
                    if ( LoopRepeat )
                    {
                        Restart();
                    }
                    else
                    {
                        enabled = false;
                        return;
                    }
                }

                _currentController = SubControllers[ _nextControllerIndex++ ];
                _currentController.enabled = true;
            }

            _currentController.UpdateActorIntent( actor );
        }
    }

    [ Serializable ]
    public class ControllerList : InspectorList<BossControllerBase>
    {
    }
}