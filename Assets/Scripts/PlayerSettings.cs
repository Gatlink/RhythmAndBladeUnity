using System.Runtime.Remoting.Messaging;
using ActorStates;
using UnityEngine;

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
    [ SerializeField ]
    private float _bodyRadius;

    [ SerializeField ]
    private float _railStickiness;

    [ Header( "Grounded" ) ]
    [ SerializeField ]
    private float _groundedMovementSpeed;

    [ SerializeField ]
    private float _groundedMoveInertia;

    [ Header( "Fall" ) ]
    [ SerializeField ]
    private float _fallMovementSpeed;

    [ SerializeField ]
    private float _fallMoveInertia;

    [ SerializeField ]
    private float _gravity;

    [ SerializeField ]
    private float _maxFallVelocity;

    [ Header( "Jump" ) ]
    [ SerializeField ]
    private float _jumpDuration;

    [ SerializeField ]
    private float _jumpMovementSpeed;

    [ SerializeField ]
    private float _jumpMoveInertia;

    [ SerializeField ]
    [ ClampedCurve ]
    private AnimationCurve _jumpHeightCurve = AnimationCurve.Linear( 0, 0, 1, 1 );

    [ SerializeField ]
    private float _jumpHeight;

    [ Header( "Wall Slide" ) ]
    [ SerializeField ]
    private float _wallStickiness;

    [ SerializeField ]
    private float _wallSlideGravity;

    [ SerializeField ]
    private float _maxWallSlideVelocity;

    [ SerializeField ]
    private float _timeToUnstickFromWall;

    [ Header( "Wall Jump" ) ]
    [ SerializeField ]
    [ Range( 0, 1 ) ]
    private float _wallJumpAirControlTiming;

    [ Header( "Dash" ) ]
    [ SerializeField ]
    private float _dashDuration;

    [ SerializeField ]
    private AnimationCurve _dashPositionCurve;

    [ SerializeField ]
    private float _dashLength;

    [ SerializeField ]
    [ Range( 0, 1 ) ]
    private float _dashJumpTiming;

    [ Header( "Dash Jump" ) ]
    [ SerializeField ]
    [ Range( 0, 1 ) ]
    private float _dashJumpAirControlTiming;

    [ SerializeField ]
    private float _dashJumpDuration;

    [ SerializeField ]
    private float _dashJumpHeight;

    [SerializeField]
    private float _dashJumpInitialMovementSpeed;

    #region ACCESSORS

    public float GroundedMovementSpeed
    {
        get { return _groundedMovementSpeed; }
    }

    public float RailStickiness
    {
        get { return _railStickiness; }
    }

    public float GroundedMoveInertia
    {
        get { return _groundedMoveInertia; }
    }

    public float FallMovementSpeed
    {
        get { return _fallMovementSpeed; }
    }

    public float FallMoveInertia
    {
        get { return _fallMoveInertia; }
    }

    public float BodyRadius
    {
        get { return _bodyRadius; }
    }

    public float Gravity
    {
        get { return _gravity; }
    }

    public float MaxFallVelocity
    {
        get { return _maxFallVelocity; }
    }

    public float JumpDuration
    {
        get { return _jumpDuration; }
    }

    public float JumpMovementSpeed
    {
        get { return _jumpMovementSpeed; }
    }

    public float JumpMoveInertia
    {
        get { return _jumpMoveInertia; }
    }

    public AnimationCurve JumpHeightCurve
    {
        get { return _jumpHeightCurve; }
    }

    public float JumpHeight
    {
        get { return _jumpHeight; }
    }

    public float WallStickiness
    {
        get { return _wallStickiness; }
    }

    public float WallSlideGravity
    {
        get { return _wallSlideGravity; }
    }

    public float MaxWallSlideVelocity
    {
        get { return _maxWallSlideVelocity; }
    }

    public float TimeToUnstickFromWall
    {
        get { return _timeToUnstickFromWall; }
    }

    public float WallJumpAirControlTiming
    {
        get { return _wallJumpAirControlTiming; }
    }

    public float DashDuration
    {
        get { return _dashDuration; }
    }

    public AnimationCurve DashPositionCurve
    {
        get { return _dashPositionCurve; }
    }

    public float DashLength
    {
        get { return _dashLength; }
    }

    public float DashJumpTiming
    {
        get { return _dashJumpTiming; }
    }

    public float DashJumpAirControlTiming
    {
        get { return _dashJumpAirControlTiming; }
    }

    public float DashJumpDuration
    {
        get { return _dashJumpDuration; }
    }

    public float DashJumpHeight
    {
        get { return _dashJumpHeight; }
    }

    public float DashJumpInitialMovementSpeed
    {
        get { return _dashJumpInitialMovementSpeed; }
    }

    #endregion
}