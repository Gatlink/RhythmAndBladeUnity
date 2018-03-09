using System;
using Gamelogic.Extensions;
using Gamelogic.Extensions.Algorithms;

namespace Controllers
{
    public class BossControllerManager : GLMonoBehaviour, IActorController<BossActor>
    {
        public bool Randomize;
        public bool LoopRepeat;

        public ControllerList SubControllers;

        private int _nextControllerIndex = 0;
        private BossControllerBase _currentController;
       
        // ReSharper disable once Unity.RedundantEventFunction
        private void Start()
        {            
        }

        public bool Enabled
        {
            get { return enabled; }
        }

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

        public void UpdateActorIntent( BossActor actor )
        {
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