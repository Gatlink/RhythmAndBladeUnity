﻿namespace ActorStates.Player
{
    public abstract class PlayerFixedHorizontalMovementState : FixedHorizontalMovementStateBase
    {
        protected readonly PlayerActor Actor;
        protected readonly PlayerSettings PlayerSettings;

        public PlayerFixedHorizontalMovementState( PlayerActor actor ) : base( actor.Mobile )
        {
            Actor = actor;
            PlayerSettings = PlayerSettings.Instance;
        }

        protected override IActorState GetNextState()
        {
            if ( !Actor.GetComponent<ActorHealth>().IsAlive )
            {
                return new DeathState( Actor );
            }

            return Mobile.CheckGround()
                ? (IActorState) new GroundedState( Actor )
                : new FallState( Actor );
        }
    }
}