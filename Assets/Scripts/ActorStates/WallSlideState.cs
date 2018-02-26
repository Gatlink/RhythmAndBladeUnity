using Gamelogic.Extensions;
using UnityEngine;

namespace ActorStates
{
    public class WallSlideState : ActorStateBase
    {
        private readonly Vector2 _wallDirection;
        private float _unstickInhibition;

        public WallSlideState( Actor actor, Vector2 wallNormal ) : base( actor )
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

            var velocity = mob.CurrentVelocity;

            // apply gravity
            velocity.y -= PlayerSettings.WallSlideGravity * Time.deltaTime;
            velocity.y = Mathf.Max( -PlayerSettings.MaxWallSlideVelocity, velocity.y );

            mob.CurrentVelocity = velocity;

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

            // default move
            mob.Move( wallMovement );

            // check damages
            var harmfull = Actor.Health.CheckDamages();
            if ( harmfull != null )
            {
                return new HurtState( Actor, harmfull );
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
                return new DashState( Actor );
            }

            if ( Actor.CheckJump() )
            {
                return new JumpState( Actor, PlayerSettings.WallJump );
            }

            return null;
        }
    }
}