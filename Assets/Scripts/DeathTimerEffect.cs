using Gamelogic.Extensions;
using UnityEngine;

public class DeathTimerEffect : GLMonoBehaviour
{
    public float FadeDuration = 0.4f;

    [ Range( 0, 1 ) ]
    public float EffectAlpha = 0.5f;

    private Texture2D _vignetteTexture;

    private void Awake()
    {
        _vignetteTexture = Resources.Load<Texture2D>( "Vignette" );
    }

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
        var cameraFade = CameraFade.Instance;

        var texture = cameraFade.FadeTexture;
        var color = cameraFade.FadeColor;

        cameraFade.FadeColor = dealsDamage ? Color.red : Color.white;
        cameraFade.FadeTexture = _vignetteTexture;
        CameraFade.FadeFrom( EffectAlpha, FadeDuration, () =>
        {
            cameraFade.FadeColor = color;
            cameraFade.FadeTexture = texture;
        } );
        if ( dealsDamage )
        {
            CameraShake.ScreenShake( 1, FadeDuration );
        }
    }
}