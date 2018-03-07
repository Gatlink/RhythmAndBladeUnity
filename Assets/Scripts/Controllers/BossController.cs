using System;
using Gamelogic.Extensions;
using Gamelogic.Extensions.Algorithms;
using UnityEngine;

namespace Controllers
{
    public class BossController : GLMonoBehaviour, IActorController<BossActor>
    {
        private enum Action
        {
            Stands = 0,
            Move,
            Jump,
            Charge,
            Attack,
            Count
        }

        public float CloseRangeThreshold;
        public float ActionDurationMean = 1;
        public float ActionDurationStdDeviation = 1;

        private IGenerator<Action> _actionGenerator;
        private IGenerator<float> _randomDurationGenerator;
        private float _nextStateTimeout;

        private Mobile _player;
        private Boss1Settings _settings;

        private void Start()
        {
            _settings = Boss1Settings.Instance;
            _player = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<Mobile>();
            _actionGenerator = Generator.UniformRandomInt( (int) Action.Count ).Cast<Action>();
            _randomDurationGenerator = Generator.GaussianRandomFloat( ActionDurationMean, ActionDurationStdDeviation )
                .Where( v => v > 0 );
            NextAction();
        }

        public bool Enabled
        {
            get { return enabled; }
        }

        public void UpdateActorIntent( BossActor actor )
        {
            switch ( _actionGenerator.Current )
            {
                case Action.Stands:
                    break;
                case Action.Move:
                    var delta = _player.BodyPosition.x - actor.Mobile.BodyPosition.x;
                    if ( Mathf.Abs( delta ) < CloseRangeThreshold )
                    {
                        _nextStateTimeout = 0;
                    }

                    actor.DesiredMovement = Mathf.Sign( delta );
                    break;
                case Action.Jump:
                    actor.DesiredJumpAttack = true;
                    break;
                case Action.Charge:
                    actor.DesiredCharge = true;
                    break;
                case Action.Attack:
                    actor.DesiredAttack = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _nextStateTimeout -= Time.deltaTime;
            if ( _nextStateTimeout <= 0 )
            {
                NextAction();
            }
        }

        private void NextAction()
        {
            _actionGenerator.Next();
            _randomDurationGenerator.Next();
            switch ( _actionGenerator.Current )
            {
                case Action.Stands:
                    _nextStateTimeout = _randomDurationGenerator.Current;
                    break;
                case Action.Move:
                    _nextStateTimeout = _randomDurationGenerator.Current;
                    break;
                case Action.Jump:
                    _nextStateTimeout = _settings.JumpAttackDuration;
                    break;
                case Action.Charge:
                    _nextStateTimeout = _settings.ChargeDuration;
                    break;
                case Action.Attack:
                    _nextStateTimeout = _randomDurationGenerator.Current;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDrawGizmosSelected()
        {
            DrawRange( CloseRangeThreshold, Color.blue );
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