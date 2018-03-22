using UnityEngine;

namespace ActorStates.Boss
{
    public class StrikeGroundState : BossFixedTimeStateBase
    {
        public StrikeGroundState( BossActor actor ) : base( actor )
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Actor.Mobile.CancelMovement();

            SpawnWave( -1 );
            SpawnWave( 1 );
        }

        protected override float TotalDuration
        {
            get { return Settings.StrikeGroundDuration; }
        }

        protected override IActorState GetNextState()
        {
            return Actor.Mobile.CheckGround()
                ? (IActorState) new GroundedState( Actor )
                : new FallState( Actor );
        }

        private void SpawnWave( float direction )
        {
            var pos = Actor.transform.position;
            var wave = Object.Instantiate( Resources.Load<GameObject>( "Shock Wave" ), pos, Quaternion.identity )
                .transform;


            float distance;
            Actor.Mobile.ProbeWall( direction, out distance, 100 );

            var duration = Mathf.Max( 0.2f, distance / Settings.ShockWaveVelocity );
            DefaultCoroutineHost.Instance.Tween( 0, distance * direction, duration,
                    Settings.ShockWaveTrajectory, v => wave.position = pos + Vector3.right * v )
                .Then( () => Object.Destroy( wave.gameObject ) );
        }
    }
}