using System;
using System.Collections;
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
    public bool UseUnscaledTime = true;

    private Color _currentColor = Color.clear;
    private Color _targetColor = Color.clear;
    private float _timeRemaining;
    private Action _completeAction;

    // Draw the texture and perform the fade:
    private void OnGUI()
    {
        if ( Event.current.type != EventType.Repaint ) return;

        if ( _timeRemaining > 0 )
        {
            var dt = UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timeRemaining = Mathf.Max( 0, _timeRemaining - dt );

            if ( _timeRemaining > 0 )
            {
                _currentColor = Color.Lerp( _currentColor, _targetColor, Mathf.Clamp01( dt / _timeRemaining ) );
            }
            else
            {
                _currentColor = _targetColor;

                if ( _completeAction != null )
                {
                    // setting _completeAction to null after calling it could lead to the deletion
                    // of a new complete callback set during a call to Fade{to|From} made inside the action.
                    var action = _completeAction;
                    _completeAction = null;
                    action();
                }
            }
        }

        // Only draw the texture when the alpha value is greater than 0:
        if ( _currentColor.a > 0 )
        {
            GUI.color = _currentColor;
            GUI.depth = FadeGUIDepth;
            GUI.DrawTexture( new Rect( 0, 0, Screen.width, Screen.height ), Instance.FadeTexture,
                ScaleMode.StretchToFill, true );
        }
    }

    public static void FadeTo( float alpha, float fadeDuration, Action onComplete = null )
    {
        if ( Instance._completeAction != null )
        {
            Debug.LogWarning( "Callback won't be called since fade has not complete and an other one is requested." );
            Instance._completeAction = null;
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if ( fadeDuration == 0 )
        {
            Instance._targetColor = Instance._currentColor = Instance.FadeColor;
            Instance._targetColor.a = Instance._currentColor.a = alpha;
            Instance._timeRemaining = 0;
            if ( onComplete != null ) onComplete();
        }
        else
        {
            Instance._targetColor = Instance.FadeColor;
            Instance._targetColor.a = alpha;
            Instance._timeRemaining = fadeDuration;
            Instance._completeAction = onComplete;
        }
    }

    public static IEnumerator FadeToCoroutine( float alpha, float fadeDuration )
    {
        var complete = false;
        FadeTo( alpha, fadeDuration, () => complete = true );
        while ( !complete )
        {
            yield return null;
        }
    }

    public static void FadeFrom( float alpha, float fadeDuration, Action onComplete = null )
    {
        if ( Instance._completeAction != null )
        {
            Debug.LogWarning( "Callback won't be called since fade has not complete and an other one is requested." );
            Instance._completeAction = null;
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if ( fadeDuration == 0 )
        {
            Instance._targetColor = Instance._currentColor;
            Instance._targetColor.a = Instance._currentColor.a;
            Instance._timeRemaining = 0;
            if ( onComplete != null ) onComplete();
        }
        else
        {
            Instance._targetColor = Instance._currentColor;
            Instance._currentColor = Instance.FadeColor;
            Instance._currentColor.a = alpha;
            Instance._timeRemaining = fadeDuration;
            Instance._completeAction = onComplete;
        }
    }

    public static IEnumerator FadeFromCoroutine( float alpha, float fadeDuration )
    {
        var complete = false;
        FadeFrom( alpha, fadeDuration, () => complete = true );
        while ( !complete )
        {
            yield return null;
        }
    }
}