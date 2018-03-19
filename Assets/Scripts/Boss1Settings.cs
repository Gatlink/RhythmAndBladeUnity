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

    [ Header( "General" ) ]
    public float CombatRangeThreshold = 1.4f;
    public float CloseRangeThreshold = 2.8f;
    public float MidRangeThreshold = 5.6f;

    [ Header( "Fall" ) ]
    public float Gravity = 40;

    public float MaxFallVelocity = 60;

    [ Header( "Grounded" ) ]
    public float GroundedMovementSpeed = 5;

    public float GroundedMoveInertia = 0.1f;

    [ Header( "Jump" ) ]
    [ Tooltip( "Minimum Jump duration" ) ]
    public float JumpMinDuration;

    [ Tooltip( "Jump horizontal speed" ) ]
    public float JumpHorizontalSpeed;

    [ Tooltip( "Height (Y) trajectory from t=0 to t=Duration" ) ]
    public Easing JumpHeightTrajectory;

    [ Tooltip( "Lateral (X) trajectory from t=0 to t=Duration" ) ]
    public Easing JumpMovementTrajectory;

    [ Tooltip( "Jump height (max Y)" ) ]
    public float JumpHeight;

    [ Header( "JumpAttack" ) ]
    [ Tooltip( "Duration of jump preparation" ) ]
    public float PrepareJumpDuration;

    [ Tooltip( "Actual jump duration" ) ]
    public float JumpAttackDuration = 1.5f;

    [ Tooltip( "Jump max height (max Y)" ) ]
    public float MaxJumpAttackHeight = 10;

    [ Tooltip( "Height (Y) trajectory from t=0 to t=Duration" ) ]
    public Easing JumpAttackHeightTrajectory = new Easing();

    [ Tooltip( "Lateral (X) trajectory from t=0 to t=Duration" ) ]
    public Easing JumpAttackMovementTrajectory = new Easing();

    [ Header( "Dive" ) ]
    public float DiveGravity;

    public float DiveMaxFallVelocity;

    [ Header( "Strike Ground" ) ]
    public float StrikeGroundDuration;

    public float ShockWaveDistance = 6;
    public float ShockWaveDuration = 1;

    [ Header( "Charge" ) ]
    public float ChargeDuration = 0.75f;

    public Easing ChargeTrajectory = new Easing();

    [ Header( "Attacks" ) ]
    public AttackSetting Attack1;

    public AttackSetting Attack2;

    public AttackSetting Attack3;


    [ Header( "Hurt" ) ]
    public float HurtDuration;

    public float HurtDriftLength;

    public Easing HurtDriftTrajectory = new Easing();

    [ Serializable ]
    public struct AttackSetting
    {
        [ Tooltip( "Total duration" ) ]
        public float Duration;

        [ Tooltip( "Horizontal distance traveled during whole attack" ) ]
        public float HorizontalMovementLength;

        [ Tooltip( "Horizontal (X) trajectory from t=0 to t=Duration" ) ]
        public Easing Trajectory;

        public float ComboWindowStartTime;

        public float ComboWindowEndTime;
    }
}