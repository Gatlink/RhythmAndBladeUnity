using UnityEngine;

namespace ActorStates.Boss
{
    public class FallState : BossActorStateBase
    {
        public FallState( BossActor actor ) : base( actor )
        {
        }

        public override IActorState Update()
        {
            var mob = Actor.Mobile;
            mob.UpdateDirection( mob.CurrentVelocity.x );

            // apply gravity
            var verticalVelocity = mob.CurrentVelocity.y - Settings.Gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max( verticalVelocity, -Settings.MaxFallVelocity );
            mob.SetVerticalVelocity( verticalVelocity );

            // default move
            mob.Move();

            if ( mob.CheckGround( ignorePassThrough: true ) )
            {
                return new GroundedState( Actor );
            }

            return null;
        }
    }
}