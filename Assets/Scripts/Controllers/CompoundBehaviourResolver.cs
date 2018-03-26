using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using ActorStates.Boss;
using Gamelogic.Extensions.Algorithms;

namespace Controllers
{
    public class CompoundBehaviourResolver
    {
        private readonly BossBehaviourController _controller;
        private bool _actorCriticalHurtFlagSet;
        private List<string> _behaviours;

        public CompoundBehaviourResolver( BossBehaviourController controller )
        {
            _controller = controller;
        }

        public IEnumerable GetResolver( CompoundBehaviourNode node )
        {
            var actor = _controller.GetComponent<BossActor>();
            _behaviours = new List<string>();
            if ( node.Randomize )
            {
                _behaviours.AddRange( node.GetChildNodes().Zip( node.GetChildMultiplicators(), Enumerable.Repeat )
                    .SelectMany( enumerable => enumerable ) );
            }
            else
            {
                _behaviours.AddRange( node.GetChildNodes() );
            }

            BossBehaviour.Log( actor + " starts behaviour " + node.Name, actor );

            do
            {
                foreach ( var unused in GetOneLoopResolver( node ) )
                {
                    if ( node.UseHealthEndCondition )
                    {
                        var currentHealth = actor.GetComponent<ActorHealth>().CurrentHitCount;
                        if ( !_actorCriticalHurtFlagSet && currentHealth <= node.HealthEndConditionLimit + 1 )
                        {
                            _actorCriticalHurtFlagSet = true;
                            actor.SetNextHurtIsCritical();
                        }

                        if ( currentHealth <= node.HealthEndConditionLimit )
                        {
                            // wait for actor to go out of next hurt state
                            foreach ( var unused1 in _controller.WaitForStateExit<CriticalHurtState>( actor ) )
                            {
                                yield return null;
                            }

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

            foreach ( var behaviourNodeGuid in _behaviours )
            {
                foreach ( var unused in _controller.GetBehaviourNodeResolver( behaviourNodeGuid ) )
                {
                    yield return null;
                }
            }
        }
    }
}