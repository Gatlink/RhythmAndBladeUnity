using Gamelogic.Extensions;
using UnityEngine;

public interface IHarmfull
{
    int Damage { get; }
    float Recoil { get; }
    Optional<Vector2> RecoilDirectionOverride { get; }
    GameObject GameObject { get; }
    bool SkipHurtState { get; }
    bool TeleportToLastCheckpoint { get; }
}