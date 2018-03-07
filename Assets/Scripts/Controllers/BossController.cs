using Gamelogic.Extensions;
using UnityEngine;

namespace Controllers
{
    public class BossController : GLMonoBehaviour, IActorController<BossActor>
    {
        public float CloseRangeThreshold;
        public float FarRangeThreshold;

        private Mobile _player;

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Mobile>();
        }

        public bool Enabled
        {
            get { return enabled; }
        }

        public void UpdateActorIntent( BossActor actor )
        {
            var mob = actor.GetComponent<Mobile>();

            var bossPos = mob.transform.position;
            var playerPos = _player.transform.position;

            actor.DesiredMovement = 0;

            actor.DesiredJumpAttack = false;

            actor.DesiredAttack = false;

            actor.DesiredCharge = false;
        }

        private void OnValidate()
        {
            if ( FarRangeThreshold < CloseRangeThreshold )
            {
                FarRangeThreshold = CloseRangeThreshold;
            }
        }

        private void OnDrawGizmosSelected()
        {
            DrawRange( CloseRangeThreshold, Color.blue );
            DrawRange( FarRangeThreshold, Color.cyan );
        }

        private void DrawRange( float range, Color color, float height = 10 )
        {
            Gizmos.color = color;

            var leftRange = transform.position + Vector3.left * range;
            Gizmos.DrawLine( leftRange, leftRange + Vector3.up * height );

            var rightRange = transform.position + Vector3.right * range;
            Gizmos.DrawLine( rightRange, rightRange + Vector3.up * height );
        }
    }
}