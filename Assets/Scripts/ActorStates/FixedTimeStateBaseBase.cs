using UnityEngine;

namespace ActorStates
{
    public abstract class FixedTimeStateBaseBase : ActorStateBase
    {
        private float _timeRemaining;

        protected FixedTimeStateBaseBase( Actor actor ) : base( actor )
        {
        }

        protected abstract float TotalDuration { get; }

        protected float ElapsedTime
        {
            get { return TotalDuration - _timeRemaining; }
        }

        protected float NormalizedTime
        {
            get { return ElapsedTime / TotalDuration; }
        }

        public override void OnEnter()
        {
            _timeRemaining = TotalDuration;
        }

        protected IActorState ChangeStateOnFinish()
        {
            _timeRemaining -= Time.deltaTime;
            if ( _timeRemaining <= 0 )
            {
                if ( Actor.CheckGround() )
                {
                    return new GroundedState( Actor );
                }
                else
                {
                    return new FallState( Actor );
                }
            }

            return null;
        }
    }
}