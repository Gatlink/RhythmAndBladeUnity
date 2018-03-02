using Gamelogic.Extensions;
using UnityEngine;

public class CameraShake : Singleton<CameraShake>
{
    private ShakeBehaviour _shakeBehaviour;

    private void Awake()
    {
        _shakeBehaviour = Camera.main.GetComponent<ShakeBehaviour>();
    }

    public static Coroutine ScreenShake( float strength = 1, float duration = 1 )
    {
        return Instance._shakeBehaviour.Shake( strength, duration );
    }
}