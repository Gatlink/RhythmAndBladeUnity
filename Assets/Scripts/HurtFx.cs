using Gamelogic.Extensions;
using UnityEngine;

public class HurtFx : Singleton<HurtFx>
{
    public float FadeDuration = 0.4f;

    [ Range( 0, 1 ) ]
    public float EffectAlpha = 0.5f;

    private Texture2D _vignetteTexture;

    private void Awake()
    {
        _vignetteTexture = Resources.Load<Texture2D>( "Vignette" );
    }

    public void TriggerHurtFx( Color color, bool cameraShake )
    {
        var cameraFade = CameraFade.Instance;

        var prevTexture = cameraFade.FadeTexture;
        var prevColor = cameraFade.FadeColor;

        cameraFade.FadeColor = color;
        cameraFade.FadeTexture = _vignetteTexture;
        CameraFade.FadeFrom( EffectAlpha, FadeDuration, () =>
        {
            cameraFade.FadeColor = prevColor;
            cameraFade.FadeTexture = prevTexture;
        } );
        if ( cameraShake )
        {
            CameraShake.ScreenShake( 1, FadeDuration );
        }
    }
}