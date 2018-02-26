using UnityEngine;

public interface IMoving
{
    Vector2 CurrentVelocity { get; set; }
    Vector2 CurrentAcceleration { get; set; }
}