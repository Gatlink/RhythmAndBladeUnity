using System;
using UnityEngine;

[ Serializable ]
public class Easing
{
    public enum EasingType
    {
        Curve,
        Function
    }

    public EasingType Type;

    public AnimationCurve Curve;

    public EasingFunction.Ease Function;

    private EasingFunction.Function _easingFunctionImpl;
    private EasingFunction.Ease _cachedFunction;

    private EasingFunction.Function EasingFunctionImpl
    {
        get
        {
            if ( _easingFunctionImpl == null || _cachedFunction != Function )
            {
                _easingFunctionImpl = EasingFunction.GetEasingFunction( Function );
                _cachedFunction = Function;
            }

            return _easingFunctionImpl;
        }
    }

    public Easing() : this( EasingFunction.Ease.Linear )
    {
    }

    public Easing( AnimationCurve curve )
    {
        Type = EasingType.Curve;
        Curve = curve;
        Function = EasingFunction.Ease.Linear;
    }

    public Easing( EasingFunction.Ease function )
    {
        Type = EasingType.Function;
        Function = function;
        Curve = AnimationCurve.Linear( 0, 0, 1, 1 );
    }
    
    public static implicit operator Func<float, float, float, float>(Easing self)
    {
        return self.Eval;
    }

    public float Eval( float t )
    {
        return Eval( 0, 1, t );
    }
    
    public float Eval( float a, float b, float t )
    {
        if ( Type == EasingType.Curve )
        {
            return a + ( b - a ) * Curve.Evaluate( t );
        }
        else
        {
            return EasingFunctionImpl( a, b, t );
        }
    }
}