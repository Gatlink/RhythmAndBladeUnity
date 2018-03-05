using UnityEngine;

public interface IDestructible
{
    void Hit( HitInfo hitInfo );
}

public struct HitInfo
{
    public readonly uint Id;

    public readonly int Damage;

    public readonly GameObject Source;

    public HitInfo( uint id, GameObject source, int damage = 1 )
    {
        Id = id;
        Source = source;
        Damage = damage;
    }

    private static uint _nextId = 1;

    public static uint GenerateId()
    {
        return _nextId++;
    }
}