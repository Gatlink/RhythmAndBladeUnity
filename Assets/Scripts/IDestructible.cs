public interface IDestructible
{
    void Hit( HitInfo hitInfo );
}

public struct HitInfo
{
    public readonly uint Id;

    public readonly int Damage;

    public HitInfo( uint id, int damage = 1 )
    {
        Id = id;
        Damage = damage;
    }

    private static uint _nextId = 1;

    public static uint GenerateId()
    {
        return _nextId++;
    }
}