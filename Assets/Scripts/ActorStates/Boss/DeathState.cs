using UnityEngine;

namespace ActorStates.Boss
{
    public class DeathState : BossActorStateBase
    {
        public DeathState( BossActor actor ) : base( actor )
        {
        }

        public override IActorState Update()
        {
            var mob = Actor.Mobile;
            if ( !mob.CheckGround( ignorePassThrough: true ) )
            {
                var settings = PlayerSettings.Instance;

                // apply gravity
                var verticalVelocity = mob.CurrentVelocity.y - settings.Gravity * Time.deltaTime;
                verticalVelocity = Mathf.Max( verticalVelocity, -settings.MaxFallVelocity );
                mob.SetVerticalVelocity( verticalVelocity );

                // default move
                mob.Move();
            }

            return null;
        }
    }
}