using UnityEngine;

public class CameraFadeAtStart : MonoBehaviour
{
    public float FadeDuration = 2;

    void Start()
    {
        CameraFade.FadeFrom( 1, FadeDuration );
    }
}