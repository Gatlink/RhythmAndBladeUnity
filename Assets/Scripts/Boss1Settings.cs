using System;
using UnityEngine;

[ CreateAssetMenu ]
public class Boss1Settings : ScriptableObject
{
    private static Boss1Settings _instance;

    public static Boss1Settings Instance
    {
        get
        {
            if ( _instance == null )
            {
                _instance = Resources.Load<Boss1Settings>( "Boss 1 Settings" );
            }

            return _instance;
        }
    }
    
    [ Header( "Grounded" ) ]
    public float GroundedMovementSpeed = 10;

    public float GroundedMoveInertia = 0.1f;

    [ Header( "Attacks" ) ]
    public AttackSetting Attack1;
    
    public AttackSetting Attack2;
    
    
    [ Serializable ]
    public struct AttackSetting
    {
        [ Tooltip( "Total duration" ) ]
        public float Duration;

        [ Tooltip( "Horizontal distance traveled during whole attack" ) ]
        public float HorizontalMovementLength;

        [ Tooltip( "Horizontal (X) trajectory from t=0 to t=Duration" ) ]
        public AnimationCurve MovementCurve;

        public float ComboWindowStartTime;
        
        public float ComboWindowEndTime;
    }}
