using System;
using Gamelogic.Extensions;
using UnityEngine;

public class CameraFade : Singleton<CameraFade>
{
    private void Awake()
    {
        if ( Instance.FadeTexture != null ) return;
        Instance.FadeTexture = new Texture2D( 1, 1 );
        Instance.FadeTexture.SetPixel( 0, 0, Color.white );
        Instance.FadeTexture.Apply();
    }

    public Texture2D FadeTexture;
    public Color FadeColor = Color.black;
    public int FadeGUIDepth = -1000;

    private float _alpha;
    private Coroutine _tween;
    private bool _lastTweenWasInterruptible;

    private void OnGUI()
    {
        if ( Event.current.type != EventType.Repaint ) return;

        if ( _alpha > 0 )
        {
            GUI.color = FadeColor.WithAlpha( _alpha );
            GUI.depth = FadeGUIDepth;
            GUI.DrawTexture( new Rect( 0, 0, Screen.width, Screen.height ), FadeTexture, ScaleMode.StretchToFill,
                true );
        }
    }

    private Action WrapCleanup( Action action )
    {
        return () =>
        {
            if ( action != null )
            {
                action();
            }

            _tween = null;
        };
    }

    private void Fade( float from, float to, float duration, Action onComplete = null, bool interruptible = true )
    {
        if ( _tween != null )
        {
            if ( !_lastTweenWasInterruptible )
            {
                return;
            }

            StopCoroutine( _tween );
        }

        _lastTweenWasInterruptible = interruptible;
        _tween = Tween( from, to, duration, Mathf.Lerp, v => _alpha = v )
            .Then( WrapCleanup( onComplete ) );
    }

    public static void FadeTo( float alpha, float fadeDuration, Action onComplete = null, bool interruptible = true )
    {
        Instance.Fade( Instance._alpha, alpha, fadeDuration, onComplete, interruptible );
    }

    public static void FadeFrom( float alpha, float fadeDuration, Action onComplete = null, bool interruptible = true )
    {
        Instance.Fade( alpha, 0, fadeDuration, onComplete, interruptible );
    }
}