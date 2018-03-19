using System.Collections;
using Gamelogic.Extensions.Algorithms;

namespace Controllers
{
    public class FixedScriptBossController : BossControllerBase
    {
        public bool Randomize;
        public bool LoopRepeat;

        public ActionList Script;
        
        private int _nextActionIndex;
        private IEnumerator _currentAction;

        private void OnEnable()
        {
            Restart();
        }

        private void Restart()
        {
            _nextActionIndex = 0;
            _currentAction = null;
            if ( Randomize )
            {
                Script.Shuffle();
            }
        }

        public override void UpdateActorIntent( BossActor actor )
        {
            base.UpdateActorIntent( actor );

            if ( _currentAction != null && _currentAction.MoveNext() ) return;
            
            if ( _nextActionIndex == Script.Count )
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
            
            _currentAction = ActionResolver( actor, Script[ _nextActionIndex++ ] );
        }
    }    
}