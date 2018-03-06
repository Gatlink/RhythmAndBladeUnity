using UnityEngine;

namespace ActorStates.Boss
{
    public class DiveState : BossActorStateBase    
    {
        public DiveState( BossActor actor ) : base( actor )
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Actor.Mobile.CancelHorizontalMovement();
        }

        public override IActorState Update()
        {
            var mob = Actor.Mobile;
            mob.UpdateDirection( mob.CurrentVelocity.x );

            // apply gravity
            var verticalVelocity = mob.CurrentVelocity.y - Settings.DiveGravity * Time.deltaTime;
            verticalVelocity = Mathf.Max( verticalVelocity, -Settings.DiveMaxFallVelocity );
            mob.SetVerticalVelocity( verticalVelocity  );

            // default move
            mob.Move();

            if ( mob.CheckGround() )
            {
                return new StrikeGroundState( Actor );
            }

            return null;
        }
    }
}