using UnityEngine;

    public abstract class EventTrigger : MonoBehaviour
    {
        public GameObject[] TargetGameObjects;
        public Behaviour[] TargetBehaviours;

        [ Tooltip( "Also turn target state back to on (off) when event does no longer hold" ) ]
        public bool AutoReset;

        [ Tooltip( "Set Targets to this state when event holds" ) ]
        public bool TargetState = true;

        private bool _eventTriggered;

        private void Update()
        {
            if ( !_eventTriggered)
            {
                if ( IsEventTriggered() )
                {
                    _eventTriggered = true;
                    SetTargetsState( TargetState );
                }
            }
            else if ( AutoReset && !IsEventTriggered() )
            {
                _eventTriggered = false;
                SetTargetsState( !TargetState );
            }
        }

        private void SetTargetsState( bool on )
        {
            foreach ( var component in TargetBehaviours )
            {
                component.enabled = on;
            }

            foreach ( var obj in TargetGameObjects )
            {
                obj.SetActive( on );
            }
        }

        protected abstract bool IsEventTriggered();
    }
