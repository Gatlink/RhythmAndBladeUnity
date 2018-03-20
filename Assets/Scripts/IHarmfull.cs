using UnityEngine;

public interface IHarmfull
{
    int Damage { get; }
    float Recoil { get; }
    GameObject GameObject { get; }
    bool SkipHurtState { get; }
    bool TeleportToLastCheckpoint { get; }
}