using UnityEngine;

namespace ActorStates
{
    public abstract class FixedTimeStateBase : PlayerActorStateBase
    {
        private float _timeRemaining;

        protected FixedTimeStateBase( PlayerActor actor ) : base( actor )
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

        protected IActorState<PlayerActor> ChangeStateOnFinish()
        {
            _timeRemaining -= Time.deltaTime;
            if ( _timeRemaining <= 0 )
            {
                if ( Actor.Mobile.CheckGround() )
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