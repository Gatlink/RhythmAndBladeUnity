using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates.Player
{
    public class WallSlideState : PlayerActorStateBase
    {
        private readonly Vector2 _wallDirection;
        private float _unstickInhibition;

        public WallSlideState( PlayerActor actor, Vector2 wallNormal ) : base( actor )
        {
            _wallDirection = wallNormal;
        }

        public override void OnEnter()
        {
            Actor.Mobile.UpdateDirection( _wallDirection.Dot( Vector2.right ) );
            Actor.Mobile.CancelMovement();
            _unstickInhibition = PlayerSettings.TimeToUnstickFromWall;
            Actor.ResetDash();
            Actor.ResetJump();
            Actor.ResetAttack();
        }

        public override IActorState Update()
        {
            var mob = Actor.Mobile;

            Vector2 normal;
            Collider2D collider;
            if ( !mob.CheckWallProximity( -mob.Direction, out normal, out collider ) )
            {
                Debug.LogWarning( "Should not happen except during the very first frame" );
                return new FallState( Actor );
            }

            // update horizontal velocity is 0
            mob.CancelHorizontalMovement();

            // add wall movement if there is any
            var wallMovement = Vector2.zero;
            if ( collider.gameObject.CompareTag( Tags.Moving ) )
            {
                var moving = collider.GetInterfaceComponentInParent<IMoving>();
                if ( moving == null )
                {
                    Debug.LogError( "IMoving component not found in " + collider, collider );
                }
                else
                {
                    wallMovement = moving.CurrentVelocity;
                }
            }
            else
            {
                // apply gravity
                var verticalVelocity = mob.CurrentVelocity.y - PlayerSettings.WallSlideGravity * Time.deltaTime;
                verticalVelocity = Mathf.Max( verticalVelocity, -PlayerSettings.MaxWallSlideVelocity );
                mob.SetVerticalVelocity( verticalVelocity );
            }

            // default move
            mob.Move( wallMovement );

            // check damages
            var harmfull = Actor.CheckHurts();
            if ( harmfull != null )
            {
                return harmfull;
            }

            // check if player wants to unstick from wall
            if ( Actor.DesiredMovement * mob.Direction > 0 )
            {
                _unstickInhibition -= Time.deltaTime;
                if ( _unstickInhibition <= 0 )
                {
                    return new FallState( Actor );
                }
            }
            else
            {
                _unstickInhibition = PlayerSettings.TimeToUnstickFromWall;
            }

            if ( !Actor.GetComponent<ActorHealth>().IsAlive )
            {
                return new DeathState( Actor );
            }

            if ( !mob.CheckWallProximity( -mob.Direction ) )
            {
                return new FallState( Actor );
            }

            if ( mob.CheckGround() )
            {
                return new GroundedState( Actor );
            }

            if ( Actor.CheckDash() )
            {
                return new DashState( Actor, useCurrentDirection: true );
            }

            if ( Actor.CheckJump() )
            {
                return new JumpState( Actor, PlayerSettings.WallJump );
            }

            return null;
        }
    }
}