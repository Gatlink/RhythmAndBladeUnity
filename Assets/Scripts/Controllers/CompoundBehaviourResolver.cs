using System.Collections;
using System.Collections.Generic;
using Gamelogic.Extensions.Algorithms;

namespace Controllers
{
    public class CompoundBehaviourResolver
    {
        private readonly BossBehaviourController _controller;
        private bool _actorCriticalHurtFlagSet;
        private List<BehaviourNode> _behaviours;

        public CompoundBehaviourResolver( BossBehaviourController controller )
        {
            _controller = controller;
        }

        public IEnumerable GetResolver( CompoundBehaviourNode node )
        {
            var actor = _controller.GetComponent<BossActor>();
            _behaviours = new List<BehaviourNode>();
            _behaviours.AddRange( node.ChildNodes );

            do
            {
                foreach ( var unused in GetOneLoopResolver( node ) )
                {
                    if ( node.HealthEndCondition.UseValue )
                    {
                        var currentHealth = actor.GetComponent<ActorHealth>().CurrentHitCount;
                        if ( !_actorCriticalHurtFlagSet && currentHealth <= node.HealthEndCondition.Value + 1 )
                        {
                            _actorCriticalHurtFlagSet = true;
                            actor.SetNextHurtIsCritical();
                        }

                        if ( currentHealth <= node.HealthEndCondition.Value )
                        {
                            yield break;
                        }
                    }

                    yield return null;
                }
            } while ( node.LoopRepeat );
        }

        private IEnumerable GetOneLoopResolver( CompoundBehaviourNode node )
        {
            if ( node.Randomize )
            {
                _behaviours.Shuffle();
            }

            foreach ( var behaviourNode in _behaviours )
            {
                foreach ( var unused in _controller.GetBehaviourNodeResolver( behaviourNode ) )
                {
                    yield return null;
                }
            }
        }
    }
}