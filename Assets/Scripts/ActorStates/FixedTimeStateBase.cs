using UnityEngine;

namespace ActorStates
{
    public abstract class FixedTimeStateBase : ActorStateBase
    {
        private float _timeRemaining;

        protected abstract float TotalDuration { get; }

        protected float ElapsedTime
        {
            get { return TotalDuration - _timeRemaining; }
        }

        protected float NormalizedTime
        {
            get { return ElapsedTime / TotalDuration; }
        }

        protected void TerminateState()
        {
            _timeRemaining = 0;
        }

        public override void OnEnter()
        {
            _timeRemaining = TotalDuration;
        }

        public override IActorState Update()
        {
            _timeRemaining -= Time.deltaTime;
            if ( _timeRemaining <= 0 )
            {
                return GetNextState();
            }

            return null;
        }

        protected abstract IActorState GetNextState();
    }
}