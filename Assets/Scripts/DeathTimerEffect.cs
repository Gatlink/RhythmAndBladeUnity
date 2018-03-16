using Gamelogic.Extensions;
using UnityEngine;

public class DeathTimerEffect : GLMonoBehaviour
{
    private void OnEnable()
    {
        GetRequiredComponent<DeathTimer>().TickEvent += TickEventHandler;
    }

    private void OnDisable()
    {
        var deathTimer = GetRequiredComponent<DeathTimer>();
        if ( deathTimer != null )
        {
            deathTimer.TickEvent -= TickEventHandler;
        }
    }

    private void TickEventHandler( bool dealsDamage )
    {
        if ( dealsDamage )
        {
            HurtFx.Instance.TriggerHurtFx( Color.red, true );
        }
        else
        {
            HurtFx.Instance.TriggerHurtFx( Color.white, false );
        }
    }
}