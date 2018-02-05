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
    [ FormerlySerializedAs( "_bodyRadius" ) ]
    public float BodyRadius = 1;

    [ FormerlySerializedAs( "_railStickiness" ) ]
    public float RailStickiness = 0.2f;

    [ Header( "Grounded" ) ]
    [ FormerlySerializedAs( "_groundedMovementSpeed" ) ]
    public float GroundedMovementSpeed = 15;

    [ FormerlySerializedAs( "_groundedMoveInertia" ) ]
    public float GroundedMoveInertia = 0.05f;

    [ Header( "Fall" ) ]
    [ FormerlySerializedAs( "_fallMovementSpeed" ) ]
    public float FallMovementSpeed = 10;

    [ FormerlySerializedAs( "_fallMoveInertia" ) ]
    public float FallMoveInertia = 0.1f;

    [ FormerlySerializedAs( "_gravity" ) ]
    public float Gravity = 40;

    [ FormerlySerializedAs( "_maxFallVelocity" ) ]
    public float MaxFallVelocity = 60;

    [ Header( "Wall Slide" ) ]
    [ FormerlySerializedAs( "_wallStickiness" ) ]
    public float WallStickiness = 0.1f;

    [ FormerlySerializedAs( "_wallSlideGravity" ) ]
    public float WallSlideGravity = 100;

    [ FormerlySerializedAs( "_maxWallSlideVelocity" ) ]
    public float MaxWallSlideVelocity = 3;

    [ FormerlySerializedAs( "_timeToUnstickFromWall" ) ]
    public float TimeToUnstickFromWall = 0.06f;

    [ Header( "Dash" ) ]
    [ FormerlySerializedAs( "_dashDuration" ) ]
    public float DashDuration = 0.2f;

    [ FormerlySerializedAs( "_dashPositionCurve" ) ]
    public AnimationCurve DashPositionCurve = AnimationCurve.Linear( 0, 0, 1, 1 );

    [ FormerlySerializedAs( "_dashLength" ) ]
    public float DashLength = 4;

    [ Range( 0, 1 ) ]
    [ FormerlySerializedAs( "_dashJumpTiming" ) ]
    public float DashJumpTiming = 0.33f;

    [ Header( "Jump" ) ]    
    [ Range( 0, 1 ) ]
    public float JumpAirControlTiming = 0.66f;
    
    [ FormerlySerializedAs( "_jumpDuration" ) ]
    public float JumpDuration = 0.3f;

    [ FormerlySerializedAs( "_jumpMovementSpeed" ) ]
    public float JumpMovementSpeed = 10;

    [ FormerlySerializedAs( "_jumpMoveInertia" ) ]
    public float JumpMoveInertia = 0.2f;

    [ ClampedCurve ]
    [ FormerlySerializedAs( "_jumpHeightCurve" ) ]
    public AnimationCurve JumpHeightCurve = AnimationCurve.Linear( 0, 0, 1, 1 );

    [ FormerlySerializedAs( "_jumpHeight" ) ]
    public float JumpHeight = 5;

    public float JumpInitialMovementSpeed = 10;   
    
    [ Header( "Wall Jump" ) ]
    [ Range( 0, 1 ) ]
    [ FormerlySerializedAs( "_wallJumpAirControlTiming" ) ]
    public float WallJumpAirControlTiming = 0.66f;

    public float WallJumpDuration = 0.3f;
    
    public float WallJumpMovementSpeed = 10;
    
    public float WallJumpMoveInertia = 0.2f;

    [ ClampedCurve ]
    public AnimationCurve WallJumpHeightCurve = AnimationCurve.Linear( 0, 0, 1, 1 );

    public float WallJumpHeight = 5;
    
    public float WallJumpInitialMovementSpeed = 10;
        
    [ Header( "Dash Jump" ) ]
    [ Range( 0, 1 ) ]
    [ FormerlySerializedAs( "_dashJumpAirControlTiming" ) ]
    public float DashJumpAirControlTiming = 0.66f;

    [ FormerlySerializedAs( "_dashJumpDuration" ) ]
    public float DashJumpDuration = 0.6f;

    public float DashJumpMovementSpeed = 10;
    
    public float DashJumpMoveInertia = 0.2f;

    [ ClampedCurve ]
    public AnimationCurve DashJumpHeightCurve = AnimationCurve.Linear( 0, 0, 1, 1 );

    [ FormerlySerializedAs( "_dashJumpHeight" ) ]
    public float DashJumpHeight = 3;

    [ FormerlySerializedAs( "_dashJumpInitialMovementSpeed" ) ]
    public float DashJumpInitialMovementSpeed = 25;    
}