using UnityEngine;

namespace ActorStates.Boss
{
    public class ChargeAttackState : BossFixedHorizontalMovementBase
    {
        private float _distance;

        public ChargeAttackState( BossActor actor ) : base( actor )
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            var playerPosition =
                GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Mobile>().BodyPosition;
            var toPlayer = Mathf.Sign( playerPosition.x - Mobile.BodyPosition.x );

            Direction = toPlayer;
            Mobile.UpdateDirection( toPlayer );

            //_distance = Mathf.Abs( toPlayer ) + 2;
            float distance;
            if ( !Mobile.ProbeWall( toPlayer, out distance, 100 ) )
            {
                Debug.LogError( "Wall not found !" );
            }

            _distance = Mathf.Max( 0, distance - 1 );
        }

        protected override float TotalDuration
        {
            get { return Settings.ChargeDuration; }
        }

        protected override float MovementLength
        {
            get { return _distance; }
        }

        protected override Easing MovementTrajectory
        {
            get { return Settings.ChargeTrajectory; }
        }

        public override IActorState Update()
        {
            ApplyHorizontalMovement();

            Mobile.Move();

            if ( Mobile.CheckWallProximity( Mobile.Direction, snap: false ) )
            {
                Mobile.CancelHorizontalMovement();
                TerminateState();
            }

            return base.Update();
        }
    }
}