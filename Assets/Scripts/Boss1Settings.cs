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
    public float GroundedMovementSpeed = 5;

    public float GroundedMoveInertia = 0.1f;

    [ Header( "JumpAttack" ) ]
    [ Tooltip( "Duration of jump preparation" ) ]
    public float PrepareJumpDuration;

    [ Tooltip( "Actual jump duration" ) ]
    public float JumpAttackDuration = 1.5f;

    [ Tooltip( "Jump max height (max Y)" ) ]
    public float MaxJumpAttackHeight = 10;

    [ Tooltip( "Height (Y) trajectory from t=0 to t=Duration" ) ]
    public AnimationCurve JumpAttackHeightCurve = AnimationCurve.Linear( 0, 0, 1, 1 );

    [ Tooltip( "Max horizontal movement allowed during jump" ) ]
    public float MaxJumpAttackMovementLength = 10;

    [ Tooltip( "Height (Y) trajectory from t=0 to t=Duration" ) ]
    public AnimationCurve JumpAttackMovementCurve = AnimationCurve.Linear( 0, 0, 1, 1 );

    [ Header( "Dive" ) ]
    public float DiveGravity;

    public float DiveMaxFallVelocity;

    [ Header( "Strike Ground" ) ]
    public float StrikeGroundDuration;

    [ Header( "Charge" ) ]
    public float ChargeDuration = 0.75f;
    
    public AnimationCurve ChargeMovementCurve = AnimationCurve.Linear( 0, 0, 1, 1 );

    public float MaxChargeMovementLength = 8;
    
    [ Header( "Attacks" ) ]
    public AttackSetting Attack1;

    public AttackSetting Attack2;

    public AttackSetting Attack3;

    [ Header( "Hurt" ) ]
    public float HurtDuration;

    public float HurtDriftLength;

    public AnimationCurve HurtDriftMovementCurve = AnimationCurve.Linear( 0, 0, 1, 1 );

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
    }
}