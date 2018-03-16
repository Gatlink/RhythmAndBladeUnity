using Gamelogic.Extensions;
using UnityEngine;

public class HurtFx : Singleton<HurtFx>
{
    private void Awake()
    {
        _fadeTexture = Resources.Load<Texture2D>( "Vignette" );
    }

    private Texture2D _fadeTexture;
    private const int FXGUIDepth = -900;

    public float EffectDuration = 0.4f;

    [ Range( 0, 1 ) ]
    public float MaxAlpha = 0.5f;

    private float _alpha;
    private Color _color;
    private Coroutine _tween;
    
    private void OnGUI()
    {
        if ( Event.current.type != EventType.Repaint ) return;

        if ( _alpha > 0 )
        {
            GUI.color = _color.WithAlpha( _alpha );
            GUI.depth = FXGUIDepth;
            GUI.DrawTexture( new Rect( 0, 0, Screen.width, Screen.height ), _fadeTexture,
                ScaleMode.StretchToFill, true );
        }
    }

    public void TriggerHurtFx( Color color, bool cameraShake )
    {
        if ( _tween != null )
        {
            StopCoroutine( _tween );
        }
        _color = color;
        _tween = Tween( MaxAlpha, 0, EffectDuration, Mathf.Lerp, v => _alpha = v );
        
        if ( cameraShake )
        {
            CameraShake.ScreenShake( 1, EffectDuration );
        }
    }
}