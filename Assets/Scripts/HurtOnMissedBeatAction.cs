using UnityEngine;

public class HurtOnMissedBeatAction : MonoBehaviour
{
    public int Damage = 1;

    private ActorHealth _health;

    private void Start()
    {
        var beatManager = BeatManager.Instance;
        if ( beatManager == null )
        {
            enabled = false;
            return;
        }

        BeatManager.Instance.BeatEvent += OnBeatEvent;
        _health = GetComponent<ActorHealth>();
    }

    private void OnBeatEvent( int beatcount, bool playerSuccess, float nextBeat, BeatManager.BeatAction nextBeatAction )
    {
        if ( !playerSuccess )
        {
            _health.AccountDamages( Damage, BeatManager.Instance.gameObject );
            HurtFx.Instance.TriggerHurtFx( Color.red, true );
        }
    }
}