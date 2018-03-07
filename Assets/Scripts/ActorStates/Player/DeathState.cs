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
                // apply gravity
                var verticalVelocity = mob.CurrentVelocity.y - PlayerSettings.Gravity * Time.deltaTime;
                verticalVelocity = Mathf.Max( verticalVelocity, -PlayerSettings.MaxFallVelocity );
                mob.SetVerticalVelocity( verticalVelocity );

                // default move
                mob.Move();
            }

            return null;
        }
    }
}