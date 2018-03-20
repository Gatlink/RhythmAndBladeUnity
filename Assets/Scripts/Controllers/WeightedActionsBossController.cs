using System;
using System.Collections;
using System.Linq;
using Gamelogic.Extensions;
using Gamelogic.Extensions.Algorithms;

namespace Controllers
{
    public class WeightedActionsBossController : BossActionControllerBase
    {
        public WeightedActionList WeightedActions;

        private IGenerator<Action> _actionGenerator;

        protected override void Awake()
        {
            base.Awake();
            if ( WeightedActions.Count == 1 )
            {
                _actionGenerator = Generator.Repeat( new Action[] { WeightedActions[ 0 ].Action } );
            }
            else
            {
                _actionGenerator = Generator.FrequencyRandomInt( WeightedActions.Select( wa => wa.Weight ) )
                    .Select( index => WeightedActions[ index ].Action );
            }
        }

        private IEnumerator _currentAction;

        public override void UpdateActorIntent( BossActor actor )
        {
            base.UpdateActorIntent( actor );
            if ( !Enabled ) return;
            
            while ( _currentAction == null || !_currentAction.MoveNext() )
            {
                _currentAction = ActionResolver( actor, _actionGenerator.Next() );
            }
        }

        [ Serializable ]
        public struct WeightedAction
        {
            public Action Action;
            public float Weight;
        }
    }

    [ Serializable ]
    public class WeightedActionList : InspectorList<WeightedActionsBossController.WeightedAction>
    {
    }
}