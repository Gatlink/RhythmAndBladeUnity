using UnityEngine;

public static class Layers
{
    public const string Ground = "Rail";
    public const string Wall = "Wall";
    public const string Destructible = "Destructible";
}

public static class Tags
{
    public const string Player = "Player";
    public const string Hitbox = "Hitbox";
}

public interface IDestructible
{
    void Hit( GameObject source );
}