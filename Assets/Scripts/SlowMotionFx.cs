using System.Collections;
using Gamelogic.Extensions;
using UnityEngine;

public class SlowMotionFx : Singleton<SlowMotionFx>
{
    public float FrozenTimescale = 0.1f;
    public float FreezeDuration = 0.1f;

    public static void Freeze()
    {
        Instance.StartCoroutine( Instance.FreezeCoroutine() );
    }

    private  IEnumerator FreezeCoroutine()
    {
        Time.timeScale = FrozenTimescale;
        
        yield return new WaitForSecondsRealtime( FreezeDuration );

        Time.timeScale = 1;
    }
}
