using ActorStates;
using ActorStates.Boss;
using Gamelogic.Extensions;
using UnityEngine;

public class BossActor : ActorBase<BossActor>
{
    [ Header( "Inputs" ) ]
    [ ReadOnly ]
    public float DesiredMovement;

    [ ReadOnly ]
    public bool DesiredAttack;

    [ ReadOnly ]
    public bool DesiredJumpAttack;

    [ ReadOnly ]
    public bool DesiredCharge;

    public Mobile Mobile { get; private set; }

    public ActorHealth Health { get; private set; }

    public bool CheckAttack()
    {
        return DesiredAttack;
    }

    public bool CheckJumpAttack()
    {
        return DesiredJumpAttack;
    }

    public bool CheckCharge()
    {
        return DesiredCharge;
    }

    protected override void ResetIntent()
    {
        DesiredMovement = 0;
        DesiredAttack = false;
        DesiredJumpAttack = false;
        DesiredCharge = false;
    }

    protected override IActorState CreateInitialState()
    {
        return new FallState( this );
    }

    private void Awake()
    {
        var settings = PlayerSettings.Instance;

        Mobile = GetRequiredComponent<Mobile>();
        Mobile.RailStickiness = settings.RailStickiness;
        Mobile.WallStickiness = settings.WallStickiness;

        Health = GetRequiredComponent<ActorHealth>();
    }
}