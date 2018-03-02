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

            var settings = PlayerSettings.Instance;
            // apply gravity
            var verticalVelocity = mob.CurrentVelocity.y - settings.Gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max( verticalVelocity, -settings.MaxFallVelocity );
            mob.SetVerticalVelocity( verticalVelocity  );

            // default move
            mob.Move();

            if ( mob.CheckGround() )
            {
                return new GroundedState( Actor );
            }

            return null;
        }
    }
}