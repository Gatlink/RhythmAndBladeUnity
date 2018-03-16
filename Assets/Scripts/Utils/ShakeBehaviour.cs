#define USE_GAMELOGIC_EXTENSION

#if USE_GAMELOGIC_EXTENSION
using Gamelogic.Extensions;
#endif

using UnityEngine;

[ CheckInspectorButtons ]
#if USE_GAMELOGIC_EXTENSION
public class ShakeBehaviour : GLMonoBehaviour
#else
public class ShakeBehaviour : MonoBehaviour
#endif
{
    [ Header( "Translation" ) ]
    public Vector3 TranslationDirection = Vector2.zero;

    public float TranslationStrength;
    public float TranslationStrengthScale = 0.05f;

    [ Header( "Rotation" ) ]
    public Vector3 RotationAxes = Vector2.zero;

    public float RotationStrength;
    public float RotationStrengthScale = 0.1f;

    [ Header( "Shake" ) ]
    public float Vibrato = 30;

    public float Smoothing = 0.01f;

    public float Strength
    {
        get { return _strength; }
        set
        {
            _strength = value;
            TranslationStrength = _strength * TranslationStrengthScale;
            RotationStrength = _strength * RotationStrengthScale;
        }
    }

    public float StepDuration
    {
        get { return 1 / Vibrato; }
    }

    private float _timeRemaining;
    private Vector3 _nextStepPosition;
    private Vector3 _nextStepRotation;
    private Vector3 _currentVelocity;
    private Vector3 _currentMomentum;
    private float _strength;

    private void Update()
    {
        if ( Vibrato > 0 && TranslationStrength > 0 )
        {
            _timeRemaining -= Time.deltaTime;

            if ( _timeRemaining <= StepDuration )
            {
                _timeRemaining += StepDuration;
                UpdateNextStep();
            }
        }
        else
        {
            UpdateNextStep();
        }

        UpdatePosition();
        UpdateRotation();
    }

    private void UpdatePosition()
    {
        var pos = transform.localPosition;
        pos = Vector3.SmoothDamp( pos, _nextStepPosition, ref _currentVelocity, Smoothing );
        transform.localPosition = pos;
    }

    private void UpdateRotation()
    {
        var eulers = transform.localRotation.eulerAngles;
        eulers.x = Mathf.SmoothDampAngle( eulers.x, _nextStepRotation.x, ref _currentMomentum.x, Smoothing );
        eulers.y = Mathf.SmoothDampAngle( eulers.y, _nextStepRotation.y, ref _currentMomentum.y, Smoothing );
        eulers.z = Mathf.SmoothDampAngle( eulers.z, _nextStepRotation.z, ref _currentMomentum.z, Smoothing );
        transform.localRotation = Quaternion.Euler( eulers );
    }

    private void UpdateNextStep()
    {
        if ( Vibrato > 0 )
        {
            if ( TranslationStrength > 0 )
            {
                _nextStepPosition = TranslationStrength * Vector3.Scale( TranslationDirection,
                                        new Vector3( Random.value * 2 - 1, Random.value * 2 - 1,
                                            Random.value * 2 - 1 ) );
            }
            else
            {
                _nextStepPosition = Vector3.zero;
            }

            if ( RotationStrength > 0 )
            {
                _nextStepRotation = RotationStrength * Vector3.Scale( RotationAxes,
                                        new Vector3( Random.value * 2 - 1, Random.value * 2 - 1,
                                            Random.value * 2 - 1 ) );
            }
            else
            {
                _nextStepRotation = Vector3.zero;
            }
        }
        else
        {
            _nextStepPosition = Vector3.zero;
            _nextStepRotation = Vector3.zero;
        }
    }

#if USE_GAMELOGIC_EXTENSION
    public Coroutine Shake( float strength, float duration )
    {
        return Tween( strength, 0, duration, EasingFunction.EaseOutQuad, v => Strength = v );
    }

    [ InspectorButton ]
    public void TestShake()
    {
        Shake( 1, 0.5f );
    }
#endif
}