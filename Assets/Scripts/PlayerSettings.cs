using System.Runtime.Remoting.Messaging;
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

    [Header("Wall Slide")]
    [SerializeField]
    private float _wallStickiness;

    [SerializeField]
    private float _wallSlideGravity;

    [SerializeField]
    private float _maxWallSlideVelocity;

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

    #endregion
}