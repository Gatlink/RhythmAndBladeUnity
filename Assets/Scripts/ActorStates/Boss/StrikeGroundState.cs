using Gamelogic.Extensions.Algorithms;
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

            DefaultCoroutineHost.Instance.Tween( 0, Settings.ShockWaveDistance * direction, Settings.ShockWaveDuration,
                EasingFunction.EaseOutQuad, v => wave.position = pos + Vector3.right * v )
                .Then( () => Object.Destroy( wave.gameObject ) );
        }
    }
}