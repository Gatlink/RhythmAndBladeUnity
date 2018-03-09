using System;
using System.Collections;
using System.Linq;
using Gamelogic.Extensions;
using Gamelogic.Extensions.Algorithms;
using UnityEngine.Serialization;

namespace Controllers
{
    public class RandomBossController : BossControllerBase
    {
        [FormerlySerializedAs("Actions")]
        public WeightedActionList WeightedActions;

        private IGenerator<Action> _actionGenerator;

        protected override void Awake()
        {
            base.Awake();
            _actionGenerator = Generator.FrequencyRandomInt( WeightedActions.Select( wa => wa.Weight ) )
                .Select( index => WeightedActions[ index ].Action );
        }

        private IEnumerator _currentAction;

        public override void UpdateActorIntent( BossActor actor )
        {
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
    public class WeightedActionList : InspectorList<RandomBossController.WeightedAction>
    {
    }
}