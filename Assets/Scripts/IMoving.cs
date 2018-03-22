using UnityEngine;

public interface IMoving
{
    bool Enabled { get; }
    Vector2 CurrentVelocity { get; set; }
    Vector2 CurrentAcceleration { get; set; }
}