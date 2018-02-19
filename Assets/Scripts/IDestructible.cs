public interface IDestructible
{
    void Hit( HitInfo hitInfo );
}

public struct HitInfo
{
    public uint Id;

    public HitInfo( uint id )
    {
        Id = id;
    }

    private static uint _nextId = 1;

    public static uint GenerateId()
    {
        return _nextId++;
    }
}