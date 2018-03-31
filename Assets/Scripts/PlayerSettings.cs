﻿using System;
using UnityEngine;
using UnityEngine.Serialization;


[ CreateAssetMenu ]
public class PlayerSettings : ScriptableObject
{
    private static PlayerSettings _instance;

    public static PlayerSettings Instance
    {
        get
        {
            if ( _instance == null )
            {
                _instance = Resources.Load<PlayerSettings>( "Player Settings" );
            }

            return _instance;
        }
    }

    [ Header( "General" ) ]
    public int InitialHitCount = 5;

    [ Header( "Hurt Recoil" ) ]
    public float HurtRecoilDuration = 0.25f;

    public float HurtRecoilDistance = 3;

    [ Header( "Grounded" ) ]
    [ FormerlySerializedAs( "_groundedMovementSpeed" ) ]
    public float GroundedMovementSpeed = 15;

    [ FormerlySerializedAs( "_groundedMoveInertia" ) ]
    public float GroundedMoveInertia = 0.05f;

    [ Header( "Fall" ) ]
    [ FormerlySerializedAs( "_fallMovementSpeed" ) ]
    public float FallMovementSpeed = 10;

    [ FormerlySerializedAs( "_fallMoveInertia" ) ]
    public float FallMoveInertia = 0.2f;

    [ FormerlySerializedAs( "_gravity" ) ]
    public float Gravity = 40;

    [ FormerlySerializedAs( "_maxFallVelocity" ) ]
    public float MaxFallVelocity = 60;

    [ Header( "Wall Slide" ) ]
    [ FormerlySerializedAs( "_wallSlideGravity" ) ]
    public float WallSlideGravity = 100;

    [ FormerlySerializedAs( "_maxWallSlideVelocity" ) ]
    public float MaxWallSlideVelocity = 3;

    [ FormerlySerializedAs( "_timeToUnstickFromWall" ) ]
    public float TimeToUnstickFromWall = 0.06f;

    [ Header( "Dash" ) ]
    [ FormerlySerializedAs( "_dashDuration" ) ]
    public float DashDuration = 0.3f;

    [ FormerlySerializedAs( "_dashPositionCurve" ) ]
    public Easing DashTrajectory = new Easing();

    [ FormerlySerializedAs( "_dashLength" ) ]
    public float DashLength = 6;

    [ Range( 0, 1 ) ]
    [ FormerlySerializedAs( "_dashJumpTiming" ) ]
    [ Tooltip( "Time after which it is possible to do a dash jump" ) ]
    public float DashJumpTiming;
    
    [ Range( 0, 1 ) ]
    [ Tooltip( "Time after which it is possible to do an attack" ) ]
    public float DashAttackTiming = 1;


    [ Header( "Jumps" ) ]
    public JumpSetting NormalJump;

    public JumpSetting WallJump;

    public JumpSetting DashJump;

    [ Header( "Attacks" ) ]
    public float MaxAirAttackVelocity = 4;

    public float AttackMoveInertia = 0.2f;

    [ FormerlySerializedAs( "AirAttackMoveInertia" ) ]
    public float AirAttackMoveDownwardInertia = 0.3f;

    public float AirAttackMoveUpwardInertia = 0.3f;

    public AttackSetting Attack1;

    public AttackSetting Attack2;

    public AttackSetting Attack3;

    [ Serializable ]
    public struct AttackSetting
    {
        [ Tooltip( "Hit phase (active hitbox phase) duration" ) ]
        public float HitDuration;

        [ Tooltip( "Combo phase (combo is possible) duration" ) ]
        public float ComboDuration;

        [ Tooltip( "Recovery phase duration (animation end)" ) ]
        public float RecoveryDuration;

        [ Tooltip( "Horizontal distance traveled during whole attack" ) ]
        public float HorizontalMovementLength;

        [ Tooltip( "Horizontal (X) trajectory from t=0 to t=Hit+Combo+Recovery" ) ]
        public Easing Trajectory;

        [ Tooltip( "Cannot trigger another attack during cooldown" ) ]
        public float Cooldown;
    }
}

[ Serializable ]
public struct JumpSetting
{
    [ Tooltip( "Jump duration (before going to fall state)" ) ]
    public float Duration;

    [ Tooltip( "Jump height (max Y)" ) ]
    public float Height;

    [ Tooltip( "Height (Y) trajectory from t=0 to t=Duration" ) ]
    public Easing HeightTrajectory;

    [ Tooltip( "Movement speed during air control" ) ]
    public float HorizontalMovementSpeed;

    [ Tooltip( "Movement inertia during air control" ) ]
    public float HorizontalMovementInertia;

    [ Range( 0, 1 ) ]
    [ Tooltip( "Normalized time after which air control is enabled" ) ]
    public float AirControlTiming;

    [ Tooltip( "Movement speed before air control is enabled" ) ]
    public float InitialMovementSpeed;

    [ Tooltip( "Can transition to wall slide state during jump" ) ]
    public bool CheckWallSlide;
}