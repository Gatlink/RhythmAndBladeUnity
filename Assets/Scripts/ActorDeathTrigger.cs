using UnityEngine;

public class ActorDeathTrigger : EventTriggerBase
{
    [ Tooltip( "Defaults to Player health when null" ) ]
    public ActorHealth TargetHealth;

    private void Start()
    {
        if ( TargetHealth == null )
        {
            TargetHealth = GameObject.FindGameObjectWithTag( Tags.Player ).GetComponent<ActorHealth>();
        }
    }

    protected override bool IsEventTriggered()
    {
        return !TargetHealth.IsAlive;
    }
}