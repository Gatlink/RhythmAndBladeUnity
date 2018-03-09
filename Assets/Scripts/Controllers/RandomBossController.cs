using System;
using System.Collections;
using Gamelogic.Extensions.Algorithms;

namespace Controllers
{
    public class RandomBossController : BossControllerBase
    {        
        public float ActionDurationMean = 1;

        public float ActionDurationStdDeviation = 1;

        private IGenerator<ActionType> _actionGenerator;

        protected override void Awake()
        {
            base.Awake();            
            _actionGenerator = Generator.UniformRandomInt( (int) ActionType.Count ).Cast<ActionType>();
        }

        private IEnumerator _currentAction;
        
        public override void UpdateActorIntent( BossActor actor )
        {
            if ( _currentAction == null || !_currentAction.MoveNext())
            {
                _currentAction = ActionResolver( actor, NextAction( actor ) );
            }
        }

        private Action NextAction( BossActor actor )
        {
            _actionGenerator.MoveNext();
            var toPlayer = Player.BodyPosition - actor.Mobile.BodyPosition;
            if ( toPlayer.magnitude > Settings.CloseRangeThreshold )
            {
                while ( _actionGenerator.Current == ActionType.Attack )
                {
                    _actionGenerator.MoveNext();
                } 
            }

            switch ( _actionGenerator.Current )
            {
                case ActionType.Stands:
                    return Action.Stand( ActionDurationMean, ActionDurationStdDeviation / 2 );
                case ActionType.Move:
                    return Action.Move( ActionDurationMean, ActionDurationStdDeviation );
                case ActionType.Jump:
                    return Action.Jump();
                case ActionType.Charge:
                    return Action.Charge();
                case ActionType.Attack:
                    return Action.Attack( ActionDurationMean, ActionDurationStdDeviation );
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}