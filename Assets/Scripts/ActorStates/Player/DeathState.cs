using UnityEngine;

namespace ActorStates.Player
{
    public class DeathState : PlayerActorStateBase
    {
        public DeathState( PlayerActor actor ) : base( actor )
        {
        }

        public override IActorState Update()
        {
            var mob = Actor.Mobile;
            if ( !mob.CheckGround() )
            {
                mob.ApplyGravity( PlayerSettings.Gravity, PlayerSettings.MaxFallVelocity );

                // default move
                mob.Move();
            }

            return null;
        }
    }
}