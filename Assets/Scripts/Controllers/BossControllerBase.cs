using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using ActorStates;
using ActorStates.Boss;
using Gamelogic.Extensions;
using Gamelogic.Extensions.Algorithms;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions.Must;

namespace Controllers
{
    public abstract class BossControllerBase : GLMonoBehaviour, IActorController<BossActor>
    {
        public string Name;
        public OptionalInt HealthEndCondition;

        public bool Enabled
        {
            get { return enabled; }
            protected set { enabled = value; }
        }

        public virtual void UpdateActorIntent( BossActor actor )
        {
            if ( HealthEndCondition.UseValue )
            {
                if ( actor.GetComponent<ActorHealth>().CurrentHitCount <= HealthEndCondition.Value )
                {
                    enabled = false;
                }
            }
        }        
    }
}