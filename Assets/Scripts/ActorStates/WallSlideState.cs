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
            Actor.UpdateDirection( _wallDirection.Dot( Vector2.right ) );
            Actor.CurrentVelocity = Vector3.zero;
            _unstickInhibition = PlayerSettings.TimeToUnstickFromWall;
            Actor.ResetDash();
            Actor.ResetJump();
            Actor.ResetAttack();
        }

        public override IActorState Update()
        {
            Vector2 normal;
            Collider2D collider;
            if ( !Actor.CheckWallProximity( -Actor.Direction, out normal, out collider ) )
            {
                Debug.LogWarning( "Should not happen except during the very first frame" );
                return new FallState( Actor );
            }

            // update horizontal velocity is 0
            Actor.CurrentVelocity.x = 0;

            // apply gravity
            Actor.CurrentVelocity.y -= PlayerSettings.WallSlideGravity * Time.deltaTime;
            Actor.CurrentVelocity.y = Mathf.Max( -PlayerSettings.MaxWallSlideVelocity, Actor.CurrentVelocity.y );

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
            Actor.Move( ( (Vector2) Actor.CurrentVelocity + wallMovement ) * Time.deltaTime );

            Actor.CheckWallCollisions();

            // check damages
            var harmfull = Actor.CheckDamages();
            if ( harmfull != null )
            {
                return new HurtState( Actor, harmfull );
            }

            // check if player wants to unstick from wall
            if ( Actor.DesiredMovement * Actor.Direction > 0 )
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

            if ( !Actor.CheckWallProximity( -Actor.Direction ) )
            {
                return new FallState( Actor );
            }

            if ( Actor.CheckGround() )
            {
                return new GroundedState( Actor );
            }

            if ( Actor.CheckDash() )
            {
                return new DashState( Actor );
            }

            if ( Actor.CheckJump() )
            {
                return new WallJumpState( Actor );
            }

            return null;
        }
    }
}